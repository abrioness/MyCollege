function Message(mensaje, icon = '', title = '', allowOutsideClick = false, html = false, align = 'justify', backdrop = false, backColor = 'rgba(224,225,226,1)', confirmButtonText = 'Aceptar', confirmButtonColor = '#32A525', ir = false, url = '') {

    try {

        Atributos = {
            title: title,
            icon: icon,
            confirmButtonText: confirmButtonText,
            confirmButtonColor: confirmButtonColor,
            allowOutsideClick: allowOutsideClick
        };

        if (html) {
            Atributos.html = "<div style = text-align:" + align + "> " + mensaje + " </div>";
        }

        if (!html) {
            Atributos.text = mensaje;
        }

        if (backdrop) {
            Atributos.backdrop = backColor;
        }

        if (ir) {
            Swal.fire(Atributos).then((result) => { window.location.replace(url) }); t
            return;
        }
        Swal.fire(Atributos);
    } catch (e) {
        console.log(e);
    }

}
function Confirmar(mensaje, boton,Site=false,title = '', iconColor = '#C22F00', cancelButtonColor = '#18BEE3', confirmButtonText = 'Confirmar', confirmButtonColor = '#F27474', reverseButtons = true) {

    try {
        Atributos = {
            text: mensaje,
            title: title,
            icon: 'question',
            iconColor: iconColor,
            cancelButtonText: 'Cancelar',
            cancelButtonColor: cancelButtonColor,
            confirmButtonText: confirmButtonText,
            confirmButtonColor: confirmButtonColor,
            reverseButtons: reverseButtons,
            showCancelButton: true
        };

        Swal.fire(Atributos).then((result) => {
            if (result.isConfirmed) {
                if (!Site) {
                    __doPostBack('ctl00$ContentPlaceHolder$' + boton, '');
                    return;
                }
                __doPostBack('ctl00$'+boton, '');
            }
            else {
                Swal.fire(
                    '',
                    'Operación Cancelada!!!',
                    'warning'
                )
            }
        });
    } catch (e) {
        console.log(e);
    }
}
function Confirm(mensaje, API, Route, title = '', iconColor = '#C22F00', cancelButtonColor = '#18BEE3', confirmButtonText = 'Confirmar', confirmButtonColor = '#F27474', reverseButtons = true) {

    try {
        Atributos = {
            text: mensaje,
            title: title,
            icon: 'question',
            iconColor: iconColor,
            cancelButtonText: 'Cancelar',
            cancelButtonColor: cancelButtonColor,
            confirmButtonText: confirmButtonText,
            confirmButtonColor: confirmButtonColor,
            reverseButtons: reverseButtons,
            showCancelButton: true
        };

        Swal.fire(Atributos).then((result) => {
            if (result.isConfirmed) {
                API(Route);
            }
            else {
                Swal.fire(
                    '',
                    'Operación Cancelada!!!',
                    'warning'
                )
            }
        });
    } catch (e) {
        console.log(e);
    }
}

function APIReturnMessage(Route) {
    $.ajax({
        type: "POST",
        url: Route,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {

            if (response.d.Respuesta) {
                Message(response.d.Mensaje, 'success');
                return;
            }
            Message(response.d.Mensaje, 'error');
            return;
        },
        error: function (error) {
            Message('La Operación no fue ejecutada', 'error', '', false, false, '', false, '', 'OK','#f27474')
        }
    });
}