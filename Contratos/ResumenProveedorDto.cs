namespace Contratos
{
    // Resumen de compras de UN proveedor (para la lista y los gráficos)
    public class ResumenProveedorDto
    {
        public Guid ProveedorId { get; set; }
        public int CantidadCompras { get; set; }
        public decimal TotalComprado { get; set; }
        public DateTime? UltimaCompra { get; set; }
    }
}