using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs;
using WebApplication1.Interfaces;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VentaController : ControllerBase
    {
        private readonly IVentaInterface _ventaService;

        public VentaController(IVentaInterface ventaService)
        {
            _ventaService = ventaService;
        }

        [HttpGet]
        public IActionResult Listar()
        {
            var ventas = _ventaService.ObtenerTodos();
            return Ok(ventas);
        }

        [HttpGet("{id:guid}")]
        public IActionResult ObtenerPorId(Guid id)
        {
            var venta = _ventaService.ObtenerPorId(id);
            if (venta == null)
                return NotFound("Venta no encontrada.");
            return Ok(venta);
        }

        [HttpPost]
        public async Task<IActionResult> Registrar([FromBody] VentaDto ventaDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _ventaService.RegistrarVentaAsync(ventaDto);
            if (resultado.StartsWith("Error"))
                return BadRequest(resultado);

            return Ok(new { Mensaje = resultado });
        }
    }
}