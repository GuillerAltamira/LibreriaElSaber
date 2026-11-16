
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs
{
    public class DetalleCompraDto
    {
        [Required(ErrorMessage = "El ID del producto es requerido")]
        public Guid ProductoId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Cantidad { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "El costo unitario debe ser mayor a 0")]
        public decimal CostoUnitario { get; set; }
    }
}
