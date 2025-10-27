$(document).ready(function () {
    $('#btnLimpiar').click(function () {
        // Limpiar formulario completo
        $('form')[0].reset();

        // O limpiar campos específicos
        $('.form-control').val('');
        $('input[type="checkbox"]').prop('checked', false);
        $('input[type="radio"]').prop('checked', false);
    });
});