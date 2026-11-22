namespace Contratos
{
    public class HistorialCompraDto
    {
        public Guid CompraId { get; set; }
        public Guid Id
        {
            get => CompraId;
            set => CompraId = value;
        }

        public string NumeroFactura { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string NombreProveedor { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public int CantidadItems { get; set; }
        public List<DetalleHistorialDto> Detalles { get; set; } = new();
    }

    public class DetalleHistorialDto
    {
        public string Producto { get; set; } = string.Empty;
        public string NombreProducto
        {
            get => Producto;
            set => Producto = value;
        }

        public int Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }
        public decimal PrecioVentaActual { get; set; }
        public decimal MargenGanancia => PrecioVentaActual - CostoUnitario;
        public decimal Subtotal { get; set; }
    }
}