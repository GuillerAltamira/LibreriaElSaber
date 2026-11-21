using WebApplication1.Data;
using WebApplication1.DTOs;
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
            return _context.productos.ToList();
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

            var nuevo = new Producto
            {
                Id = Guid.NewGuid(),
                Nombre = dto.Nombre,
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

            producto.Nombre = dto.Nombre;
            producto.Descripcion = dto.Descripcion;
            producto.Precio = dto.Precio;
            producto.Stock = dto.Stock;
            producto.ImagenUrl = dto.ImagenUrl;
            producto.CategoriaId = dto.CategoriaId;

            _context.SaveChanges();
            return "Producto actualizado correctamente.";
        }

        public bool Eliminar(Guid id)
        {
            var producto = _context.productos.Find(id);
            if (producto == null)
                return false;

            _context.productos.Remove(producto);
            _context.SaveChanges();
            return true;
        }
    }
}