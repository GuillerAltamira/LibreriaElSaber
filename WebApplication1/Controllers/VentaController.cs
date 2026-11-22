using Microsoft.AspNetCore.Mvc;
using Contratos;
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

        [HttpGet("{id:guid}/factura")]
        public async Task<IActionResult> ObtenerFactura(Guid id)
        {
            var factura = await _ventaService.ObtenerFacturaPorVentaIdAsync(id);
            if (factura == null)
                return NotFound("Factura no encontrada para esta venta.");
            return Ok(factura);
        }

        [HttpPost]
        public async Task<IActionResult> Registrar([FromBody] VentaDto ventaDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _ventaService.RegistrarVentaConResultadoAsync(ventaDto);
            if (!resultado.Exito)
                return BadRequest(new { Mensaje = resultado.Mensaje });

            return Ok(resultado);
        }
    }
}