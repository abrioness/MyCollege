// Evita doble envío en POST sin cancelar el guardado (no deshabilitar el botón en el mismo evento submit).
(function () {
    if (window.WebColegioFormInit)
        return;
    window.WebColegioFormInit = true;
    function esPost(form) {
        return ((form.getAttribute('method') || 'get').toLowerCase() === 'post');
    }

    function textoDe(el) {
        if (!el)
            return '';
        if (el.tagName === 'INPUT')
            return (el.value || el.getAttribute('value') || '').toString();
        return ((el.innerText || el.textContent || '') + ' ' + (el.getAttribute('title') || '')).toString();
    }

    function accionFormulario(form, submitter) {
        var action = ((submitter && submitter.getAttribute('formaction')) || form.getAttribute('action') || window.location.pathname || '').toLowerCase();
        var clases = ((form.className || '') + ' ' + (submitter && submitter.className || '')).toLowerCase();
        var texto = (textoDe(submitter) + ' ' + action + ' ' + clases).toLowerCase();
        return { action: action, texto: texto };
    }

    function omitirConfirmacion(form, submitter) {
        if (!form || form.classList.contains('js-sin-confirmacion') || form.dataset.noConfirm === '1')
            return true;
        if (submitter && (submitter.classList.contains('js-sin-confirmacion') || submitter.dataset.noConfirm === '1'))
            return true;
        var info = accionFormulario(form, submitter);
        var ruta = ((window.location.pathname || '') + ' ' + info.action).toLowerCase();
        if (/(^|\/)login(\/login)?\/?(\s|$)/.test(ruta)
            || /recuperarpassword|restablecerpassword/.test(ruta)
            || /(^|\/)logout(\/|\s|$)/.test(ruta))
            return true;
        var btn = textoDe(submitter).toLowerCase();
        if (btn && /buscar|filtrar|exportar|limpiar filtro/.test(btn) && !/guardar|anular|actualizar|eliminar/.test(btn))
            return true;
        return false;
    }

    function mensajeConfirmacion(form, submitter) {
        var personalizado = (submitter && (submitter.getAttribute('data-mensaje') || submitter.getAttribute('data-confirm-mensaje')))
            || form.getAttribute('data-mensaje')
            || form.getAttribute('data-confirm-mensaje');
        var info = accionFormulario(form, submitter);
        var t = info.texto;
        var tipo = (submitter && submitter.getAttribute('data-confirm-tipo')) || form.getAttribute('data-confirm-tipo') || '';

        if (!tipo) {
            if (/anular|eliminar|delete|desactivar/.test(t))
                tipo = 'anular';
            else if (/cerrar\s*caja|cierre de caja/.test(t))
                tipo = 'cerrar';
            else if (/enviar|campaña|whatsapp|correo/.test(t) && /aviso|whatsapp|email|correo|campaña|seleccion/.test(t))
                tipo = 'enviar';
            else if (/edit|actualiz|update|guardar cambios/.test(t))
                tipo = 'actualizar';
            else
                tipo = 'guardar';
        }

        if (tipo === 'anular') {
            return {
                titulo: 'Confirmar anulación',
                texto: personalizado || '¿Anular este registro? Quedará sin efecto.',
                confirmar: 'Sí, anular',
                icono: 'warning',
                color: '#dc3545'
            };
        }
        if (tipo === 'cerrar') {
            return {
                titulo: 'Confirmar cierre de caja',
                texto: personalizado || '¿Cerrar caja ahora? Revise el arqueo antes de continuar.',
                confirmar: 'Sí, cerrar caja',
                icono: 'warning',
                color: '#0d6efd'
            };
        }
        if (tipo === 'enviar') {
            return {
                titulo: 'Confirmar envío',
                texto: personalizado || '¿Enviar ahora?',
                confirmar: 'Sí, enviar',
                icono: 'question',
                color: '#198754'
            };
        }
        if (tipo === 'actualizar') {
            return {
                titulo: 'Confirmar actualización',
                texto: personalizado || '¿Guardar los cambios de este registro?',
                confirmar: 'Sí, actualizar',
                icono: 'question',
                color: '#0d6efd'
            };
        }
        return {
            titulo: 'Confirmar registro',
            texto: personalizado || '¿Guardar esta información?',
            confirmar: 'Sí, guardar',
            icono: 'question',
            color: '#0d6efd'
        };
    }

    function mostrarConfirmacion(cfg) {
        if (typeof Swal !== 'undefined' && typeof Swal.fire === 'function') {
            try {
                var ret = Swal.fire({
                    title: cfg.titulo,
                    text: cfg.texto,
                    icon: cfg.icono,
                    showCancelButton: true,
                    confirmButtonText: cfg.confirmar,
                    cancelButtonText: 'Cancelar',
                    confirmButtonColor: cfg.color,
                    reverseButtons: true,
                    focusConfirm: true
                });
                if (ret && typeof ret.then === 'function') {
                    return ret.then(function (result) {
                        return !!(result && (result.isConfirmed === true || result.value === true));
                    });
                }
            } catch (ex) { }
        }
        return Promise.resolve(window.confirm(cfg.titulo + '\n' + cfg.texto));
    }

    function esAccionAnular(form, submitter) {
        var tipo = (submitter && submitter.getAttribute('data-confirm-tipo')) || form.getAttribute('data-confirm-tipo') || '';
        if (tipo === 'anular')
            return true;
        return /anular|eliminar|delete|desactivar/.test(accionFormulario(form, submitter).texto);
    }

    function reenviarFormulario(form, submitter) {
        form.dataset.confirmado = '1';
        // Anular/eliminar: envío nativo. Evita que el segundo submit se bloquee
        // (requestSubmit + validación jQuery o botón desconectado tras el aviso).
        if (esAccionAnular(form, submitter)) {
            form.dataset.envioEnCurso = '1';
            mostrarProgreso();
            bloquearBotones(form);
            HTMLFormElement.prototype.submit.call(form);
            return;
        }
        var btn = submitter
            && submitter.isConnected
            && form.contains(submitter)
            && submitter.form === form
            ? submitter
            : null;
        try {
            if (typeof form.requestSubmit === 'function') {
                if (btn)
                    form.requestSubmit(btn);
                else
                    form.requestSubmit();
                return;
            }
        } catch (ex) { }
        form.dataset.envioEnCurso = '1';
        mostrarProgreso();
        bloquearBotones(form);
        HTMLFormElement.prototype.submit.call(form);
    }

    function overlay() {
        var el = document.getElementById('webcolegio-progreso');
        if (el)
            return el;
        el = document.createElement('div');
        el.id = 'webcolegio-progreso';
        el.innerHTML = '<div class="spinner" role="status" aria-label="Procesando"></div><div>Procesando…</div>';
        document.body.appendChild(el);
        return el;
    }

    function mostrarProgreso() {
        overlay().classList.add('visible');
    }

    function ocultarProgreso() {
        var el = document.getElementById('webcolegio-progreso');
        if (el)
            el.classList.remove('visible');
    }

    function botonesEnvio(form) {
        return form.querySelectorAll('button[type="submit"], input[type="submit"]');
    }

    function bloquearBotones(form) {
        botonesEnvio(form).forEach(function (btn) {
            if (btn.dataset.bloqueoDoble === '1')
                return;
            btn.dataset.bloqueoDoble = '1';
            btn.dataset.textoOriginal = btn.tagName === 'INPUT' ? (btn.value || '') : btn.innerHTML;
            btn.disabled = true;
        });
    }

    function desbloquearFormulario(form) {
        if (!form)
            return;
        delete form.dataset.envioEnCurso;
        delete form.dataset.mantenerBloqueo;
        delete form.dataset.confirmado;
        botonesEnvio(form).forEach(function (btn) {
            if (btn.dataset.bloqueoDoble !== '1')
                return;
            btn.disabled = false;
            if (btn.dataset.textoOriginal != null) {
                if (btn.tagName === 'INPUT')
                    btn.value = btn.dataset.textoOriginal;
                else
                    btn.innerHTML = btn.dataset.textoOriginal;
            }
            delete btn.dataset.bloqueoDoble;
            delete btn.dataset.textoOriginal;
        });
        ocultarProgreso();
    }

    function marcarEnCurso(form, conProgreso) {
        if (!form)
            return;
        form.dataset.envioEnCurso = '1';
        if (conProgreso)
            mostrarProgreso();
    }

    window.WebColegioForm = {
        desbloquear: desbloquearFormulario,
        marcarEnCurso: marcarEnCurso,
        permitirSiguienteEnvio: function (form) {
            if (form)
                delete form.dataset.envioEnCurso;
        }
    };

    document.addEventListener('click', function (e) {
        var btn = e.target.closest('button[type="submit"], input[type="submit"]');
        if (!btn)
            return;
        var form = btn.form;
        if (!form || form.tagName !== 'FORM' || !esPost(form))
            return;
        if (form.classList.contains('js-permitir-doble-envio'))
            return;
        if (form.dataset.envioEnCurso === '1') {
            e.preventDefault();
            e.stopPropagation();
        }
    }, true);

    document.addEventListener('submit', function (e) {
        var form = e.target;
        if (!form || form.tagName !== 'FORM' || !esPost(form))
            return;
        var submitter = e.submitter || document.activeElement;
        if (submitter && submitter.form !== form)
            submitter = null;

        if (form.dataset.confirmado !== '1' && !omitirConfirmacion(form, submitter)) {
            e.preventDefault();
            e.stopPropagation();
            e.stopImmediatePropagation();
            mostrarConfirmacion(mensajeConfirmacion(form, submitter)).then(function (ok) {
                if (ok)
                    reenviarFormulario(form, submitter);
            });
            return;
        }

        if (form.classList.contains('js-permitir-doble-envio'))
            return;
        if (form.dataset.envioEnCurso === '1') {
            e.preventDefault();
            e.stopImmediatePropagation();
            return;
        }
        form.dataset.envioEnCurso = '1';
        setTimeout(function () {
            if (e.defaultPrevented) {
                if (form.dataset.mantenerBloqueo === '1') {
                    mostrarProgreso();
                    return;
                }
                desbloquearFormulario(form);
                return;
            }
            mostrarProgreso();
            bloquearBotones(form);
        }, 0);
    }, true);

    document.addEventListener('invalid', function (e) {
        var form = e.target && e.target.form;
        if (form && form.dataset.mantenerBloqueo !== '1')
            desbloquearFormulario(form);
    }, true);
})();

// Utilidades compartidas del sitio. La inicialización de DataTables va en cada vista.
window.datatablesEspanol = {
    decimal: ",",
    thousands: ".",
    emptyTable: "No hay datos disponibles",
    info: "Mostrando _START_ a _END_ de _TOTAL_ registros",
    infoEmpty: "Mostrando 0 a 0 de 0 registros",
    infoFiltered: "(filtrado de _MAX_ registros)",
    lengthMenu: "Mostrar _MENU_ registros",
    loadingRecords: "Cargando...",
    processing: "Procesando...",
    search: "Buscar:",
    zeroRecords: "No se encontraron resultados",
    paginate: {
        first: "Primero",
        last: "Último",
        next: "Siguiente",
        previous: "Anterior"
    },
    aria: {
        sortAscending: ": activar para ordenar de forma ascendente",
        sortDescending: ": activar para ordenar de forma descendente"
    }
};
