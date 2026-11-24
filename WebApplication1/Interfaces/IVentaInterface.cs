using Contratos;
using WebApplication1.Models;

namespace WebApplication1.Interfaces
{
    public interface IVentaInterface
    {
        IEnumerable<Venta> ObtenerTodos();
        Venta? ObtenerPorId(Guid id);
        Task<string> RegistrarVentaAsync(VentaDto ventaDto);
        Task<VentaResultadoDto> RegistrarVentaConResultadoAsync(VentaDto ventaDto);
        Task<FacturaImpresaDto?> ObtenerFacturaPorVentaIdAsync(Guid ventaId);
        string? ObtenerRazonSocialPorNit(string nit);
    }
}