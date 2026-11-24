using Contratos;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Interfaces;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class CompraService : ICompraInterface
    {
        private readonly BaseDatos _context;

        public CompraService(BaseDatos context)
        {
            _context = context;
        }

        public IEnumerable<HistorialCompraDto> ObtenerTodas()
        {
            return _context.compras
                .Include(c => c.Proveedor)
                .Include(c => c.Detalles)
                    .ThenInclude(d => d.Producto)
                .OrderByDescending(c => c.Fecha)
                .Select(c => new HistorialCompraDto
                {
                    Id = c.Id,
                    NumeroFactura = c.NumeroFactura,
                    Fecha = c.Fecha,
                    NombreProveedor = c.Proveedor != null ? c.Proveedor.Nombre : "Sin proveedor",
                    Total = c.Total,
                    CantidadItems = c.Detalles.Sum(d => d.Cantidad),
                    Detalles = c.Detalles.Select(d => new DetalleHistorialDto
                    {
                        NombreProducto = d.Producto != null ? d.Producto.Nombre : "Producto eliminado",
                        Cantidad = d.Cantidad,
                        CostoUnitario = d.CostoUnitario,
                        PrecioVentaActual = d.Producto != null ? d.Producto.Precio : 0,
                        Subtotal = d.Subtotal
                    }).ToList()
                })
                .ToList();
        }

        public HistorialCompraDto? ObtenerPorId(Guid id)
        {
            return ObtenerTodas().FirstOrDefault(c => c.Id == id);
        }

        // Busca el número más alto "FC-000123" y devuelve el siguiente: "FC-000124"
        public string SiguienteNumeroFactura()
        {
            var maximo = _context.compras
                .Where(c => c.NumeroFactura.StartsWith("FC-"))
                .Select(c => c.NumeroFactura)
                .AsEnumerable()
                .Select(n => int.TryParse(n.Substring(3), out var x) ? x : 0)
                .DefaultIfEmpty(0)
                .Max();

            return $"FC-{maximo + 1:D6}";
        }

        public string Registrar(CompraDto compraDto)
        {
            if (compraDto.Detalles == null || !compraDto.Detalles.Any())
                return "Error: La compra debe tener al menos un producto.";

            var proveedor = _context.proveedores.FirstOrDefault(p => p.Id == compraDto.ProveedorId);
            if (proveedor == null)
                return "Error: El proveedor no se encuentra registrado.";

            // Número de factura automático si no lo mandan
            var numeroFactura = string.IsNullOrWhiteSpace(compraDto.NumeroFactura)
                ? SiguienteNumeroFactura()
                : compraDto.NumeroFactura.Trim();

            bool facturaExiste = _context.compras.Any(c =>
                c.ProveedorId == compraDto.ProveedorId &&
                c.NumeroFactura == numeroFactura);

            if (facturaExiste)
                return "Error: El proveedor ya tiene registrada esa factura.";

            var nuevaCompra = new Compra
            {
                Id = Guid.NewGuid(),
                ProveedorId = compraDto.ProveedorId,
                NumeroFactura = numeroFactura,
                Fecha = DateTime.Now,
                Total = 0,
                Detalles = new List<DetalleCompra>()
            };

            decimal totalCompra = 0;
            var productosComprados = new List<(Producto Producto, DetalleCompra Detalle)>();

            foreach (var item in compraDto.Detalles)
            {
                if (item.ProductoId == Guid.Empty)
                    return "Error: El ID del producto no es válido.";

                var producto = _context.productos.Find(item.ProductoId);
                if (producto == null)
                    return $"Error: No se encontró el producto con ID '{item.ProductoId}'.";

                if (item.Cantidad <= 0)
                    return $"Error: La cantidad de '{producto.Nombre}' debe ser mayor a 0.";

                if (item.CostoUnitario <= 0)
                    return $"Error: El costo unitario de '{producto.Nombre}' debe ser mayor a 0.";

                // CONTROL DE COSTOS: Evitar comprar a un costo mayor o igual al precio de venta
                if (item.CostoUnitario >= producto.Precio)
                {
                    return $"Error de Control de Costos: El costo de '{producto.Nombre}' ({item.CostoUnitario:C}) supera o iguala su precio de venta actual ({producto.Precio:C}).";
                }

                decimal subtotal = item.Cantidad * item.CostoUnitario;
                totalCompra += subtotal;

                var detalle = new DetalleCompra
                {
                    Id = Guid.NewGuid(),
                    CompraId = nuevaCompra.Id,
                    ProductoId = item.ProductoId,
                    Cantidad = item.Cantidad,
                    CostoUnitario = item.CostoUnitario,
                    Subtotal = subtotal
                };

                nuevaCompra.Detalles.Add(detalle);
                productosComprados.Add((producto, detalle));
            }

            // INGRESO DE INVENTARIO: Aumentar stock
            foreach (var item in productosComprados)
            {
                item.Producto.Stock += item.Detalle.Cantidad;
            }

            nuevaCompra.Total = totalCompra;
            _context.compras.Add(nuevaCompra);
            _context.SaveChanges();

            return $"Compra {numeroFactura} registrada exitosamente, inventario actualizado y costos verificados.";
        }
    }
}