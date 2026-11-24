namespace Contratos
{
    public class CompraDto
    {
        public Guid ProveedorId { get; set; }
        // Si viene vacío, la API genera el siguiente número (FC-000001, FC-000002...)
        public string? NumeroFactura { get; set; }
        public List<DetalleCompraDto> Detalles { get; set; } = new();
    }

    public class DetalleCompraDto
    {
        public Guid ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }
    }
}