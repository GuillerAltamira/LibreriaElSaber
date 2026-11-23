using Contratos;
using WebApplication1.Data;
using WebApplication1.Interfaces;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class UsuarioService : IUsuarioInterface
    {
        private readonly BaseDatos _context;

        public UsuarioService(BaseDatos context)
        {
            _context = context;
        }

        public UsuarioResponseDto? ValidarCredenciales(LoginDto loginDto)
        {
            var usuario = _context.usuarios
                .FirstOrDefault(u => u.NombreUsuario == loginDto.NombreUsuario
                                  && u.Password == loginDto.Password);

            if (usuario == null)
            {
                return null;
            }

            return new UsuarioResponseDto
            {
                Id = usuario.Id,
                NombreCompleto = usuario.NombreCompleto,
                NombreUsuario = usuario.NombreUsuario,
                Rol = usuario.Rol
            };
        }

        public string Registrar(RegistroUsuarioDto registroDto)
        {
            var existe = _context.usuarios.Any(u => u.NombreUsuario.ToLower() == registroDto.NombreUsuario.Trim().ToLower());
            if (existe)
            {
                return "Error: El nombre de usuario ya está en uso.";
            }

            var nuevoUsuario = new Usuario
            {
                Id = Guid.NewGuid(),
                NombreCompleto = registroDto.NombreCompleto.Trim(),
                NombreUsuario = registroDto.NombreUsuario.Trim(),
                Password = registroDto.Password,
                Rol = string.IsNullOrWhiteSpace(registroDto.Rol) ? "Usuario" : registroDto.Rol
            };

            _context.usuarios.Add(nuevoUsuario);
            _context.SaveChanges();
            return "Usuario registrado exitosamente.";
        }

        public IEnumerable<UsuarioResponseDto> ObtenerTodos()
        {
            return _context.usuarios
                .Select(u => new UsuarioResponseDto
                {
                    Id = u.Id,
                    NombreCompleto = u.NombreCompleto,
                    NombreUsuario = u.NombreUsuario,
                    Rol = u.Rol
                })
                .ToList();
        }

        public bool Eliminar(Guid id)
        {
            var usuario = _context.usuarios.Find(id);
            if (usuario == null)
            {
                return false;
            }

            _context.usuarios.Remove(usuario);
            _context.SaveChanges();
            return true;
        }
    }
}
