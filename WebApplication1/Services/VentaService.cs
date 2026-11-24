using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using Contratos;
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
                .OrderByDescending(v => v.Fecha)
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

        public async Task<FacturaImpresaDto?> ObtenerFacturaPorVentaIdAsync(Guid ventaId)
        {
            var venta = await _context.ventas
                .Include(v => v.Factura)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(v => v.Id == ventaId);

            if (venta == null || venta.Factura == null)
                return null;

            return new FacturaImpresaDto
            {
                VentaId = venta.Id,
                NumeroFactura = venta.Factura.NumeroFactura,
                FechaEmision = venta.Factura.FechaEmision,
                Nit = venta.Factura.NIT,
                RazonSocial = venta.Factura.RazonSocial,
                Total = venta.Total,
                Detalles = venta.Detalles.Select(d => new DetalleFacturaDto
                {
                    ProductoNombre = d.Producto?.Nombre ?? "Producto no disponible",
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal = d.Subtotal
                }).ToList()
            };
        }

        // Nombre con el que ya se facturó a ese NIT/CI (null si es un cliente nuevo)
        public string? ObtenerRazonSocialPorNit(string nit)
        {
            nit = nit.Trim();
            return _context.facturas
                .Where(f => f.NIT == nit)
                .OrderByDescending(f => f.FechaEmision)
                .Select(f => f.RazonSocial)
                .FirstOrDefault();
        }

        // Busca el número más alto "FAC-000123" y devuelve el siguiente: "FAC-000124"
        private string SiguienteNumeroFactura()
        {
            var maximo = _context.facturas
                .Where(f => f.NumeroFactura.StartsWith("FAC-") && f.NumeroFactura.Length == 10)
                .Select(f => f.NumeroFactura)
                .AsEnumerable()
                .Select(n => int.TryParse(n.Substring(4), out var x) ? x : 0)
                .DefaultIfEmpty(0)
                .Max();

            return $"FAC-{maximo + 1:D6}";
        }

        public async Task<VentaResultadoDto> RegistrarVentaConResultadoAsync(VentaDto ventaDto)
        {
            if (ventaDto.Detalles == null || !ventaDto.Detalles.Any())
            {
                return new VentaResultadoDto
                {
                    Exito = false,
                    Mensaje = "Error: La venta debe tener al menos un producto."
                };
            }

            // ----- Validaciones del cliente -----
            var nitCliente = (ventaDto.Nit ?? "").Trim();
            if (!System.Text.RegularExpressions.Regex.IsMatch(nitCliente, @"^\d{5,15}$"))
                return new VentaResultadoDto { Exito = false, Mensaje = "El NIT/CI solo puede tener números (de 5 a 15 dígitos)." };

            var nombreCliente = (ventaDto.RazonSocial ?? "").Trim();
            if (nombreCliente.Count(char.IsLetter) < 3)
                return new VentaResultadoDto { Exito = false, Mensaje = "El nombre o razón social debe tener al menos 3 letras." };

            // Un mismo NIT/CI siempre pertenece al mismo cliente
            var nombreRegistrado = ObtenerRazonSocialPorNit(nitCliente);
            if (nombreRegistrado != null && !string.Equals(nombreRegistrado.Trim(), nombreCliente, StringComparison.OrdinalIgnoreCase))
                return new VentaResultadoDto { Exito = false, Mensaje = $"El NIT/CI {nitCliente} ya está registrado a nombre de \"{nombreRegistrado}\"." };

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                decimal totalVenta = 0;
                var detallesVenta = new List<DetalleVenta>();

                foreach (var item in ventaDto.Detalles)
                {
                    var producto = await _context.productos.FindAsync(item.ProductoId);
                    if (producto == null)
                    {
                        return new VentaResultadoDto
                        {
                            Exito = false,
                            Mensaje = $"Error: El producto con ID {item.ProductoId} no existe."
                        };
                    }

                    if (item.Cantidad <= 0)
                    {
                        return new VentaResultadoDto
                        {
                            Exito = false,
                            Mensaje = $"Error: La cantidad de '{producto.Nombre}' debe ser mayor a 0."
                        };
                    }

                    if (producto.Stock < item.Cantidad)
                    {
                        return new VentaResultadoDto
                        {
                            Exito = false,
                            Mensaje = $"Error: Stock insuficiente para el producto '{producto.Nombre}'. Stock actual: {producto.Stock}"
                        };
                    }

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

                var razonSocial = !string.IsNullOrWhiteSpace(ventaDto.RazonSocial)
                    ? ventaDto.RazonSocial.Trim()
                    : (!string.IsNullOrWhiteSpace(ventaDto.ClienteNombre) ? ventaDto.ClienteNombre.Trim() : "Sin Nombre");

                var nit = !string.IsNullOrWhiteSpace(ventaDto.Nit) ? ventaDto.Nit.Trim() : "0";

                var nuevaFactura = new Factura
                {
                    Id = Guid.NewGuid(),
                    VentaId = nuevaVenta.Id,
                    NumeroFactura = SiguienteNumeroFactura(),   // correlativo, no se repite
                    FechaEmision = DateTime.Now,
                    NIT = nit,
                    RazonSocial = razonSocial
                };

                _context.ventas.Add(nuevaVenta);
                _context.facturas.Add(nuevaFactura);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new VentaResultadoDto
                {
                    Exito = true,
                    Mensaje = $"Venta registrada con éxito. Factura Nro: {nuevaFactura.NumeroFactura}",
                    VentaId = nuevaVenta.Id,
                    NumeroFactura = nuevaFactura.NumeroFactura
                };
            } 
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new VentaResultadoDto
                {
                    Exito = false,
                    Mensaje = $"Error interno al registrar la venta: {ex.Message}"
                };
            }
        }

        public async Task<string> RegistrarVentaAsync(VentaDto ventaDto)
        {
            var resultado = await RegistrarVentaConResultadoAsync(ventaDto);
            return resultado.Mensaje;
        }
    }
}
