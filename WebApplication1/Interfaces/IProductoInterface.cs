using Contratos;
using WebApplication1.Models;

namespace WebApplication1.Interfaces
{
    public interface IProductoInterface
    {
        IEnumerable<Producto> ObtenerTodos();
        Producto? ObtenerPorId(Guid id);
        string Crear(ProductoDto productoDto);
        string Modificar(Guid id, ProductoDto productoDto);
        string? Eliminar(Guid id);   // null = eliminado, si no devuelve el motivo
        int ConsultarStock(Guid id);
    }
}