using System.ComponentModel.DataAnnotations;

namespace Contratos
{
    public class ProductoDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(250, MinimumLength = 3, ErrorMessage = "La descripción debe tener entre 3 y 250 caracteres.")]
        public string Descripcion { get; set; } = string.Empty;

        [Range(0.01, 100000, ErrorMessage = "El precio debe ser mayor a 0.")]
        public decimal Precio { get; set; }

        [Range(0, 100000, ErrorMessage = "El stock no puede ser negativo.")]
        public int Stock { get; set; }

        public string? ImagenUrl { get; set; }

        public Guid CategoriaId { get; set; }
    }
}
