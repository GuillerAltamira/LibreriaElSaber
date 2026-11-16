using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Factura
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid VentaId { get; set; }

        [Required]
        public string NumeroFactura { get; set; } = string.Empty;

        [Required]
        public DateTime FechaEmision { get; set; }

        [Required]
        public string NIT { get; set; } = string.Empty;

        [Required]
        public string RazonSocial { get; set; } = string.Empty;

        public Venta? Venta { get; set; }
    }
}
