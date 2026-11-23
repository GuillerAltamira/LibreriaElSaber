using Microsoft.AspNetCore.Mvc;
using Contratos;
using WebApplication1.Interfaces;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompraController : ControllerBase
    {
        private readonly ICompraInterface _compraService;

        public CompraController(ICompraInterface compraService)
        {
            _compraService = compraService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_compraService.ObtenerTodas());
        }

        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            var compra = _compraService.ObtenerPorId(id);

            if (compra == null)
            {
                return NotFound("La compra no se encuentra registrada.");
            }

            return Ok(compra);
        }

        [HttpPost]
        public IActionResult Post([FromBody] CompraDto compraDto)
        {
            var resultado = _compraService.Registrar(compraDto);

            if (resultado.StartsWith("Error"))
            {
                return BadRequest(resultado);
            }

            return Ok(resultado);
        }
    }
}
