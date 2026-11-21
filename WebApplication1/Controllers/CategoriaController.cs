using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using Contratos;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
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
            var lista = _context.categorias.ToList();
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

        [HttpPost]
        public IActionResult Post(CategoriaDto dto)
        {
            var categoria = new Categoria
            {
                Id = Guid.NewGuid(),
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion
            };
            _context.categorias.Add(categoria);
            _context.SaveChanges();
            return Ok(categoria);
        }
    }
}