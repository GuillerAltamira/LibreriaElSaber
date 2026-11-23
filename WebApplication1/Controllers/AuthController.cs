using Contratos;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Interfaces;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUsuarioInterface _usuarioService;

        public AuthController(IUsuarioInterface usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Debe completar todos los campos.");
            }

            var usuario = _usuarioService.ValidarCredenciales(loginDto);
            if (usuario == null)
            {
                return Unauthorized("Nombre de usuario o contraseña incorrectos.");
            }

            return Ok(usuario);
        }

        [HttpPost("registrar")]
        public IActionResult Registrar([FromBody] RegistroUsuarioDto registroDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Verifica que todos los campos estén completos y correctos.");
            }

            var resultado = _usuarioService.Registrar(registroDto);
            if (resultado.StartsWith("Error"))
            {
                return BadRequest(resultado);
            }

            return Ok(resultado);
        }

        [HttpGet("usuarios")]
        public IActionResult ObtenerUsuarios()
        {
            return Ok(_usuarioService.ObtenerTodos());
        }

        [HttpDelete("usuarios/{id}")]
        public IActionResult EliminarUsuario(Guid id)
        {
            var eliminado = _usuarioService.Eliminar(id);
            if (!eliminado)
            {
                return NotFound("No se encontró el usuario.");
            }

            return Ok("Usuario eliminado correctamente.");
        }
    }
}
