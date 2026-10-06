// Evita doble envío en POST sin cancelar el guardado (no deshabilitar el botón en el mismo evento submit).
(function () {
    function esPost(form) {
        return ((form.getAttribute('method') || 'get').toLowerCase() === 'post');
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
