
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs
{
    public class CompraDto
    {
        [Required(ErrorMessage = "El proveedor es requerido")]
        public Guid ProveedorId { get; set; }

        [Required(ErrorMessage = "El número de factura es requerido")]
        public string NumeroFactura { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe incluir al menos un detalle de compra")]
        public List<DetalleCompraDto> Detalles { get; set; } = new List<DetalleCompraDto>();
    }
}
