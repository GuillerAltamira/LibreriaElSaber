using WebApplication1.DTOs;
using WebApplication1.Models;

namespace WebApplication1.Interfaces
{
    public interface IVentaInterface
    {
        IEnumerable<Venta> ObtenerTodos();
        Venta? ObtenerPorId(Guid id);
        Task<string> RegistrarVentaAsync(VentaDto ventaDto); 
    }
}