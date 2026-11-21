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
        bool Eliminar(Guid id);
        int ConsultarStock(Guid id);
    }
}