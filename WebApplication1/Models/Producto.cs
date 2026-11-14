using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Producto
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El nombre del producto es requerido")]
        public string? Nombre { get; set; }

        [Required(ErrorMessage = "La descripción es requerida")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "El precio es requerido")]
        public decimal Precio { get; set; }

        [Required(ErrorMessage = "El stock es requerido")]
        public int Stock { get; set; }
    }
}