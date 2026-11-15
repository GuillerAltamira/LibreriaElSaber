using WebApplication1.DTOs;
using WebApplication1.Models;

namespace WebApplication1.Interfaces
{
    public enum ResultadoProveedor { Ok, NoEncontrado, NitDuplicado }

    public interface IProveedorInterface
    {
        IEnumerable<Proveedor> ObtenerTodos();
        Proveedor? ObtenerPorId(Guid id);
        Proveedor? Crear(ProveedorDto dto);                 // null = NIT ya existe
        ResultadoProveedor Modificar(Guid id, ProveedorDto dto);
        IEnumerable<HistorialCompraDto>? ObtenerHistorialCompras(Guid proveedorId); // null = proveedor no existe
    }
}