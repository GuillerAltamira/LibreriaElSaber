
using System.Text.Json.Serialization;

namespace WebApplication1.Models
{
    public class DetalleCompra
    {
        public Guid Id { get; set; }

        public Guid CompraId { get; set; }

        [JsonIgnore]
        public Compra? Compra { get; set; }

        public Guid ProductoId { get; set; }

        public Producto? Producto { get; set; }

        public int Cantidad { get; set; }

        public decimal CostoUnitario { get; set; }

        public decimal Subtotal { get; set; }
    }
}
