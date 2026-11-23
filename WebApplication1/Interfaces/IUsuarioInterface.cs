using Contratos;

namespace WebApplication1.Interfaces
{
    public interface IUsuarioInterface
    {
        UsuarioResponseDto? ValidarCredenciales(LoginDto loginDto);
        string Registrar(RegistroUsuarioDto registroDto);
        IEnumerable<UsuarioResponseDto> ObtenerTodos();
        bool Eliminar(Guid id);
    }
}
