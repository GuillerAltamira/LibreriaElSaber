using System.ComponentModel.DataAnnotations;

namespace Contratos
{
    public class RegistroUsuarioDto
    {
        [Required(ErrorMessage = "El nombre completo es requerido")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(4, ErrorMessage = "La contraseña debe tener al menos 4 caracteres")]
        public string Password { get; set; } = string.Empty;

        public string Rol { get; set; } = "Usuario";
    }
}
