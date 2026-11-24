using System.ComponentModel.DataAnnotations;

namespace Contratos
{
    public class CategoriaDto
    {
        [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "Máximo 250 caracteres.")]
        public string? Descripcion { get; set; }
    }
}
