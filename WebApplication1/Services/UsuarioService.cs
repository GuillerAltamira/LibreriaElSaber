using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Contratos;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using WebApplication1.Data;
using WebApplication1.Interfaces;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class UsuarioService : IUsuarioInterface
    {
        private readonly BaseDatos _context;
        private readonly IConfiguration _config;
        private readonly PasswordHasher<Usuario> _hasher = new();

        public UsuarioService(BaseDatos context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public UsuarioResponseDto? ValidarCredenciales(LoginDto loginDto)
        {
            var usuario = _context.usuarios
                .FirstOrDefault(u => u.NombreUsuario == loginDto.NombreUsuario);

            if (usuario == null || !PasswordCorrecta(usuario, loginDto.Password))
            {
                return null;
            }

            return new UsuarioResponseDto
            {
                Id = usuario.Id,
                NombreCompleto = usuario.NombreCompleto,
                NombreUsuario = usuario.NombreUsuario,
                Rol = usuario.Rol,
                Token = GenerarToken(usuario)   // el "pase" para usar la API
            };
        }

        // Crea el token JWT (como un "pase" con tu nombre y tu rol que dura 8 horas)
        private string GenerarToken(Usuario usuario)
        {
            var clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Clave"]!));
            var credenciales = new SigningCredentials(clave, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                new Claim(ClaimTypes.Role, usuario.Rol)   // "Administrador" o "Usuario"
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credenciales);

            return new JwtSecurityTokenHandler().WriteToken(token);
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
                Password = _hasher.HashPassword(null!, registroDto.Password),   // encriptada
                Rol = string.IsNullOrWhiteSpace(registroDto.Rol) ? "Usuario" : registroDto.Rol
            };

            _context.usuarios.Add(nuevoUsuario);
            _context.SaveChanges();
            return "Usuario registrado exitosamente.";
        }

        // Compara la contraseña escrita con la encriptada de la BD.
        private bool PasswordCorrecta(Usuario usuario, string password)
        {
            // Usuario antiguo con la contraseña guardada sin encriptar:
            // se acepta una vez y se encripta en ese momento.
            if (usuario.Password == password)
            {
                usuario.Password = _hasher.HashPassword(usuario, password);
                _context.SaveChanges();
                return true;
            }

            try
            {
                var resultado = _hasher.VerifyHashedPassword(usuario, usuario.Password, password);
                return resultado != PasswordVerificationResult.Failed;
            }
            catch (FormatException)
            {
                return false;   // lo guardado no es una contraseña encriptada válida
            }
        }

        public IEnumerable<UsuarioResponseDto> ObtenerTodos()
        {
            return _context.usuarios
                .OrderBy(u => u.NombreCompleto)   // alfabético
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
