namespace WebApplication1.Models
{
    // Tabla intermedia: resuelve el muchos-a-muchos entre Compra y Producto
    public class DetalleCompra
    {
        public Guid Id { get; set; }
        public Guid CompraId { get; set; }             // FK
        public Compra? Compra { get; set; }
        public Guid ProductoId { get; set; }           // FK
        public Producto? Producto { get; set; }
        public int Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}