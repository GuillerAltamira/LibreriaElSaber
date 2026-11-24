using WebApplication1.Data;
using Contratos;
using WebApplication1.Interfaces;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class ProductoService : IProductoInterface
    {
        private readonly BaseDatos _context;

        public ProductoService(BaseDatos context)
        {
            _context = context;
        }

        public IEnumerable<Producto> ObtenerTodos()
        {
            return _context.productos.OrderBy(p => p.Nombre).ToList();   // alfabético
        }

        public Producto ObtenerPorId(Guid id)
        {
            return _context.productos.FirstOrDefault(p => p.Id == id)!;
        }

        public int ConsultarStock(Guid id)
        {
            var producto = _context.productos.Find(id);
            return producto != null ? producto.Stock : -1;
        }

        public string Crear(ProductoDto dto)
        {
            var categoriaExiste = _context.categorias.Any(c => c.Id == dto.CategoriaId);
            if (!categoriaExiste)
                return "Error: La categoría especificada no existe.";

            var nombre = dto.Nombre.Trim();
            if (_context.productos.Any(p => p.Nombre.Trim().ToLower() == nombre.ToLower()))
                return "Error: Ya existe un producto con ese nombre.";

            var nuevo = new Producto
            {
                Id = Guid.NewGuid(),
                Nombre = nombre,
                Descripcion = dto.Descripcion,
                Precio = dto.Precio,
                Stock = dto.Stock,
                ImagenUrl = dto.ImagenUrl,
                CategoriaId = dto.CategoriaId
            };

            _context.productos.Add(nuevo);
            _context.SaveChanges();
            return $"Producto '{nuevo.Nombre}' registrado con ID: {nuevo.Id}";
        }

        public string Modificar(Guid id, ProductoDto dto)
        {
            var producto = _context.productos.Find(id);
            if (producto == null)
                return "Error: Producto no encontrado.";

            var categoriaExiste = _context.categorias.Any(c => c.Id == dto.CategoriaId);
            if (!categoriaExiste)
                return "Error: La categoría especificada no existe.";

            var nombre = dto.Nombre.Trim();
            if (_context.productos.Any(p => p.Id != id && p.Nombre.Trim().ToLower() == nombre.ToLower()))
                return "Error: Ya existe otro producto con ese nombre.";

            producto.Nombre = nombre;
            producto.Descripcion = dto.Descripcion;
            producto.Precio = dto.Precio;
            producto.Stock = dto.Stock;
            producto.ImagenUrl = dto.ImagenUrl;
            producto.CategoriaId = dto.CategoriaId;

            _context.SaveChanges();
            return "Producto actualizado correctamente.";
        }

        public string? Eliminar(Guid id)
        {
            var producto = _context.productos.Find(id);
            if (producto == null)
                return "No se encontró el producto para eliminar.";

            // No se puede borrar si ya aparece en compras o ventas (rompería el historial)
            var usado = _context.detalleCompras.Any(d => d.ProductoId == id)
                     || _context.detallesVenta.Any(d => d.ProductoId == id);
            if (usado)
                return "No se puede eliminar: el producto ya tiene compras o ventas registradas.";

            _context.productos.Remove(producto);
            _context.SaveChanges();
            return null;
        }
    }
}