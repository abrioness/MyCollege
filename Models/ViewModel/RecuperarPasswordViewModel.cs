using System.ComponentModel.DataAnnotations;

namespace WebColegio.Models.ViewModel
{
    public class RecuperarPasswordViewModel
    {
        [Required(ErrorMessage = "Indique su usuario, cédula o correo.")]
        [Display(Name = "Usuario, cédula o correo")]
        public string Identificador { get; set; } = string.Empty;
    }

    public class RestablecerPasswordViewModel
    {
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingrese la nueva contraseña.")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        public string NuevaPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme la nueva contraseña.")]
        [DataType(DataType.Password)]
        [Compare(nameof(NuevaPassword), ErrorMessage = "Las contraseñas no coinciden.")]
        [Display(Name = "Confirmar contraseña")]
        public string ConfirmarPassword { get; set; } = string.Empty;
    }
}
