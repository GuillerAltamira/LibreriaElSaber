using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs;
using WebApplication1.Interfaces;

namespace WebApplication1.Controllers
{
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

        [HttpPost]
        public IActionResult Post(ProductoDto productoDto)
        {
            var resultado = _productoService.Crear(productoDto);
            return Ok(resultado);
        }

        [HttpPut("{id}")]
        public IActionResult Put(Guid id, ProductoDto productoDto)
        {
            var resultado = _productoService.Modificar(id, productoDto);
            if (resultado.StartsWith("Error"))
                return NotFound(resultado);
            return Ok(resultado);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var eliminado = _productoService.Eliminar(id);
            if (!eliminado)
                return NotFound("No se encontró el producto para eliminar.");
            return Ok("Producto eliminado correctamente.");
        }
    }
}