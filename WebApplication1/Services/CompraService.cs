
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs;
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

        public IEnumerable<Compra> ObtenerTodas()
        {
            return _context.compras
                .Include(c => c.Proveedor)
                .Include(c => c.Detalles)
                    .ThenInclude(d => d.Producto)
                .ToList();
        }

        public Compra? ObtenerPorId(Guid id)
        {
            return _context.compras
                .Include(c => c.Proveedor)
                .Include(c => c.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefault(c => c.Id == id);
        }

        public string Registrar(CompraDto compraDto)
        {
            if (compraDto.Detalles == null || !compraDto.Detalles.Any())
            {
                return "Error: La compra debe tener al menos un producto.";
            }

            // Verificar que exista el proveedor
            var proveedor = _context.proveedores
                .FirstOrDefault(p => p.Id == compraDto.ProveedorId);

            if (proveedor == null)
            {
                return "Error: El proveedor no se encuentra registrado.";
            }

            // Verificar que no se repita la factura del proveedor
            bool facturaExiste = _context.compras.Any(c =>
                c.ProveedorId == compraDto.ProveedorId &&
                c.NumeroFactura == compraDto.NumeroFactura);

            if (facturaExiste)
            {
                return "Error: El proveedor ya tiene registrada esa factura.";
            }

            var nuevaCompra = new Compra
            {
                Id = Guid.NewGuid(),
                ProveedorId = compraDto.ProveedorId,
                NumeroFactura = compraDto.NumeroFactura,
                Fecha = DateTime.Now,
                Total = 0,
                Detalles = new List<DetalleCompra>()
            };

            decimal totalCompra = 0;

            var productosComprados = new List<(Producto Producto, DetalleCompra Detalle)>();

            // Validar todos los productos antes de modificar el stock
            foreach (var item in compraDto.Detalles)
            {
                if (item.ProductoId == Guid.Empty)
                {
                    return "Error: El ID del producto no es válido.";
                }

                var producto = _context.productos.Find(item.ProductoId);

                if (producto == null)
                {
                    return $"Error: No se encontró el producto con ID '{item.ProductoId}'.";
                }

                if (item.Cantidad <= 0)
                {
                    return $"Error: La cantidad de '{producto.Nombre}' debe ser mayor a 0.";
                }

                if (item.CostoUnitario <= 0)
                {
                    return $"Error: El costo unitario de '{producto.Nombre}' debe ser mayor a 0.";
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

            // Aumentar el stock de cada producto comprado
            foreach (var item in productosComprados)
            {
                item.Producto.Stock += item.Detalle.Cantidad;
            }

            nuevaCompra.Total = totalCompra;

            _context.compras.Add(nuevaCompra);

            _context.SaveChanges();

            return "Compra registrada exitosamente y stock actualizado.";
        }
    }
}
