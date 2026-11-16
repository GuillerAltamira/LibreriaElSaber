
using WebApplication1.DTOs;
using WebApplication1.Models;

namespace WebApplication1.Interfaces
{
    public interface ICompraInterface
    {
        IEnumerable<Compra> ObtenerTodas();

        Compra? ObtenerPorId(Guid id);

        string Registrar(CompraDto compraDto);
    }
}
