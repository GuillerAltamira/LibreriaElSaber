using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class DetalleVenta
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid VentaId { get; set; }

        [Required]
        public Guid ProductoId { get; set; }

        [Required]
        public int Cantidad { get; set; }

        [Required]
        public decimal PrecioUnitario { get; set; }

        [Required]
        public decimal Subtotal { get; set; }

        public Venta? Venta { get; set; }

        public Producto? Producto { get; set; }
    }
}
