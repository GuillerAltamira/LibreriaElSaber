namespace WebApplication1.Models
{
    public class Compra
    {
        public Guid Id { get; set; }
        public Guid ProveedorId { get; set; }          // FK
        public Proveedor? Proveedor { get; set; }
        public DateTime Fecha { get; set; }
        public string NumeroFactura { get; set; } = string.Empty;
        public decimal Total { get; set; }

        public ICollection<DetalleCompra> Detalles { get; set; } = new List<DetalleCompra>();
    }
}