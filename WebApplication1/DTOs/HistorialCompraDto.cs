namespace WebApplication1.DTOs
{
    public class HistorialCompraDto
    {
        public Guid CompraId { get; set; }
        public DateTime Fecha { get; set; }
        public string NumeroFactura { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public List<DetalleHistorialDto> Detalles { get; set; } = new();
    }

    public class DetalleHistorialDto
    {
        public string Producto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}