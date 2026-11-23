using Contratos;

namespace WebApplication1.Interfaces
{
    public interface ICompraInterface
    {
        IEnumerable<HistorialCompraDto> ObtenerTodas();
        HistorialCompraDto? ObtenerPorId(Guid id);
        string Registrar(CompraDto compraDto);
    }
}

