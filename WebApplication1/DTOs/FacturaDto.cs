namespace WebApplication1.DTOs
{
    public class FacturaImpresaDto
    {
        public Guid VentaId { get; set; }
        public string NumeroFactura { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public string Nit { get; set; } = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public List<DetalleFacturaDto> Detalles { get; set; } = new();
    }

    public class DetalleFacturaDto
    {
        public string ProductoNombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}