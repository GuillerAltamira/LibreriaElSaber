using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Contratos;
using WebApplication1.Interfaces;

namespace WebApplication1.Controllers
{
    // Necesita token: sin login la API responde 401
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductoController : ControllerBase
    {
        private readonly IProductoInterface _productoService;

        public ProductoController(IProductoInterface productoService)
        {
            _productoService = productoService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_productoService.ObtenerTodos());
        }

        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            var producto = _productoService.ObtenerPorId(id);
            if (producto == null)
                return NotFound("El producto no se encuentra registrado.");
            return Ok(producto);
        }

        [HttpGet("stock/{id}")]
        public IActionResult ConsultarStock(Guid id)
        {
            var stock = _productoService.ConsultarStock(id);
            if (stock == -1)
                return NotFound("El producto no se encuentra registrado.");

            return Ok(new { Mensaje = "Stock consultado exitosamente", StockDisponible = stock });
        }

        [Authorize(Roles = "Administrador")]   // el Usuario solo puede ver
        [HttpPost]
        public IActionResult Post(ProductoDto productoDto)
        {
            var resultado = _productoService.Crear(productoDto);
            if (resultado.StartsWith("Error"))
                return BadRequest(resultado.Replace("Error: ", ""));
            return Ok(resultado);
        }

        [Authorize(Roles = "Administrador")]   // el Usuario solo puede ver
        [HttpPut("{id}")]
        public IActionResult Put(Guid id, ProductoDto productoDto)
        {
            var resultado = _productoService.Modificar(id, productoDto);
            if (resultado.StartsWith("Error"))
                return BadRequest(resultado.Replace("Error: ", ""));
            return Ok(resultado);
        }

        [Authorize(Roles = "Administrador")]   // el Usuario solo puede ver
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var error = _productoService.Eliminar(id);
            if (error != null)
                return BadRequest(error);
            return Ok("Producto eliminado correctamente.");
        }
    }
}