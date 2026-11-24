using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using Contratos;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    // Necesita token: sin login la API responde 401
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriaController : ControllerBase
    {
        private readonly BaseDatos _context;

        public CategoriaController(BaseDatos context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var lista = _context.categorias.OrderBy(c => c.Nombre).ToList();   // alfabético
            return Ok(lista);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            var categoria = _context.categorias.Find(id);
            if (categoria == null)
                return NotFound("La categoría no se encuentra registrada.");

            return Ok(categoria);
        }

        [Authorize(Roles = "Administrador")]   // el Usuario solo puede ver
        [HttpPost]
        public IActionResult Post(CategoriaDto dto)
        {
            var nombre = dto.Nombre.Trim();

            // No se repite el nombre (sin importar mayúsculas ni espacios)
            var existe = _context.categorias.Any(c => c.Nombre.Trim().ToLower() == nombre.ToLower());
            if (existe)
                return Conflict("Ya existe una categoría con ese nombre.");

            var categoria = new Categoria
            {
                Id = Guid.NewGuid(),
                Nombre = nombre,
                Descripcion = dto.Descripcion?.Trim()
            };
            _context.categorias.Add(categoria);
            _context.SaveChanges();
            return Ok(categoria);
        }
    }
}