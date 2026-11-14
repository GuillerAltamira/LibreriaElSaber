using Microsoft.EntityFrameworkCore;
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

        public Producto? ObtenerPorId(Guid id)
        {
            return _context.productos.Find(id);
        }

        public string Crear(ProductoDto productoDto)
        {
            var producto = new Producto
            {
                Id = Guid.NewGuid(),
                Nombre = productoDto.Nombre,
                Descripcion = productoDto.Descripcion,
                Precio = productoDto.Precio,
                Stock = productoDto.Stock
            };

            _context.productos.Add(producto);
            _context.SaveChanges();
            return "Producto registrado exitosamente en el inventario.";
        }

        public string Modificar(Guid id, ProductoDto productoDto)
        {
            var producto = _context.productos.Find(id);
            if (producto == null)
            {
                return "Error: No se encontró el producto a modificar.";
            }

            producto.Nombre = productoDto.Nombre;
            producto.Descripcion = productoDto.Descripcion;
            producto.Precio = productoDto.Precio;
            producto.Stock = productoDto.Stock;

            _context.SaveChanges();
            return "Producto modificado exitosamente.";
        }

        public bool Eliminar(Guid id)
        {
            var producto = _context.productos.Find(id);
            if (producto == null)
            {
                return false;
            }

            _context.productos.Remove(producto);
            _context.SaveChanges();
            return true;
        }

        public int ConsultarStock(Guid id)
        {
            var producto = _context.productos.Find(id);
            return producto != null ? producto.Stock : -1;
        }
    }
}