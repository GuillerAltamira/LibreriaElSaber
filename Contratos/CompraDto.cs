namespace Contratos
{
    public class CompraDto
    {
        public Guid ProveedorId { get; set; }
        public string NumeroFactura { get; set; } = string.Empty;
        public List<DetalleCompraDto> Detalles { get; set; } = new();
    }

    public class DetalleCompraDto
    {
        public Guid ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }
    }
}