using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs;
using WebApplication1.Interfaces;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class VentaService : IVentaInterface
    {
        private readonly BaseDatos _context;

        public VentaService(BaseDatos context)
        {
            _context = context;
        }

        public IEnumerable<Venta> ObtenerTodos()
        {
            return _context.ventas
                .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
                .Include(v => v.Factura)
                .ToList();
        }

        public Venta? ObtenerPorId(Guid id)
        {
            return _context.ventas
                .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
                .Include(v => v.Factura)
                .FirstOrDefault(v => v.Id == id);
        }

        public async Task<string> RegistrarVentaAsync(VentaDto ventaDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                decimal totalVenta = 0;
                var detallesVenta = new List<DetalleVenta>();

                foreach (var item in ventaDto.Detalles)
                {
                    var producto = await _context.productos.FindAsync(item.ProductoId);
                    if (producto == null)
                        return $"Error: El producto con ID {item.ProductoId} no existe.";

                    if (producto.Stock < item.Cantidad)
                        return $"Error: Stock insuficiente para el producto '{producto.Nombre}'. Stock actual: {producto.Stock}";

                    producto.Stock -= item.Cantidad;

                    decimal subtotal = producto.Precio * item.Cantidad;
                    totalVenta += subtotal;

                    detallesVenta.Add(new DetalleVenta
                    {
                        Id = Guid.NewGuid(),
                        ProductoId = producto.Id,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = producto.Precio,
                        Subtotal = subtotal
                    });
                }

                var nuevaVenta = new Venta
                {
                    Id = Guid.NewGuid(),
                    Fecha = DateTime.Now,
                    Total = totalVenta,
                    Estado = "Completada",
                    Detalles = detallesVenta
                };

                var nuevaFactura = new Factura
                {
                    Id = Guid.NewGuid(),
                    VentaId = nuevaVenta.Id,
                    NumeroFactura = "FAC-" + new Random().Next(10000, 99999),
                    FechaEmision = DateTime.Now,
                    NIT = ventaDto.Nit,
                    RazonSocial = ventaDto.RazonSocial
                };

                _context.ventas.Add(nuevaVenta);
                _context.facturas.Add(nuevaFactura);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return $"Venta registrada con éxito. Factura Nro: {nuevaFactura.NumeroFactura}";
            } 
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return $"Error interno al registrar la venta: {ex.Message}";
            }
        }
    }
}