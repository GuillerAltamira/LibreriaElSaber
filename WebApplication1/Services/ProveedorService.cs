using WebApplication1.Data;
using Contratos;
using WebApplication1.Interfaces;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class ProveedorService : IProveedorInterface
    {
        private readonly BaseDatos _context;

        public ProveedorService(BaseDatos context)
        {
            _context = context;
        }

        public IEnumerable<Proveedor> ObtenerTodos()
        {
            return _context.proveedores.OrderBy(p => p.Nombre).ToList();
        }

        public Proveedor? ObtenerPorId(Guid id)
        {
            return _context.proveedores.Find(id);
        }

        public Proveedor? Crear(ProveedorDto dto)
        {
            var nit = dto.Nit.Trim();
            if (_context.proveedores.Any(p => p.Nit == nit))
                return null;

            var proveedor = new Proveedor
            {
                Id = Guid.NewGuid(),
                Nombre = dto.Nombre.Trim(),
                Nit = nit,
                Telefono = dto.Telefono,
                Correo = dto.Correo,
                Direccion = dto.Direccion,
                Activo = dto.Activo
            };

            _context.proveedores.Add(proveedor);
            _context.SaveChanges();
            return proveedor;
        }

        public ResultadoProveedor Modificar(Guid id, ProveedorDto dto)
        {
            var proveedor = _context.proveedores.Find(id);
            if (proveedor == null)
                return ResultadoProveedor.NoEncontrado;

            var nit = dto.Nit.Trim();
            if (_context.proveedores.Any(p => p.Nit == nit && p.Id != id))
                return ResultadoProveedor.NitDuplicado;

            proveedor.Nombre = dto.Nombre.Trim();
            proveedor.Nit = nit;
            proveedor.Telefono = dto.Telefono;
            proveedor.Correo = dto.Correo;
            proveedor.Direccion = dto.Direccion;
            proveedor.Activo = dto.Activo;

            _context.SaveChanges();
            return ResultadoProveedor.Ok;
        }

        public IEnumerable<HistorialCompraDto>? ObtenerHistorialCompras(Guid proveedorId)
        {
            if (!_context.proveedores.Any(p => p.Id == proveedorId))
                return null;

            return _context.compras
                .Where(c => c.ProveedorId == proveedorId)
                .OrderByDescending(c => c.Fecha)
                .Select(c => new HistorialCompraDto
                {
                    CompraId = c.Id,
                    Fecha = c.Fecha,
                    NumeroFactura = c.NumeroFactura,
                    Total = c.Total,
                    Detalles = c.Detalles.Select(d => new DetalleHistorialDto
                    {
                        Producto = d.Producto!.Nombre ?? "",
                        Cantidad = d.Cantidad,
                        CostoUnitario = d.CostoUnitario,
                        Subtotal = d.Subtotal
                    }).ToList()
                })
                .ToList();
        }

        // Cuántas compras y cuánto se le compró a cada proveedor
        public IEnumerable<ResumenProveedorDto> ObtenerResumenCompras()
        {
            return _context.compras
                .GroupBy(c => c.ProveedorId)
                .Select(g => new ResumenProveedorDto
                {
                    ProveedorId = g.Key,
                    CantidadCompras = g.Count(),
                    TotalComprado = g.Sum(c => c.Total),
                    UltimaCompra = g.Max(c => (DateTime?)c.Fecha)
                })
                .ToList();
        }
    }
}