using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Venta
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [Required]
        public decimal Total { get; set; }

        [Required]
        public string Estado { get; set; } = "Registrada";

        public ICollection<DetalleVenta> Detalles { get; set; }
            = new List<DetalleVenta>();

        public Factura? Factura { get; set; }
    }
}
