using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs;
using WebApplication1.Interfaces;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProveedorController : ControllerBase
    {
        private readonly IProveedorInterface _proveedorService;

        public ProveedorController(IProveedorInterface proveedorService)
        {
            _proveedorService = proveedorService;
        }

        [HttpGet]
        public IActionResult Listar()
        {
            return Ok(_proveedorService.ObtenerTodos());
        }

        [HttpGet("{id:guid}")]
        public IActionResult ObtenerPorId(Guid id)
        {
            var proveedor = _proveedorService.ObtenerPorId(id);
            if (proveedor == null)
                return NotFound("El proveedor no se encuentra registrado.");
            return Ok(proveedor);
        }

        [HttpPost]
        public IActionResult Registrar(ProveedorDto dto)
        {
            var proveedor = _proveedorService.Crear(dto);
            if (proveedor == null)
                return Conflict("Ya existe un proveedor con ese NIT.");

            return CreatedAtAction(nameof(ObtenerPorId), new { id = proveedor.Id }, proveedor);
        }

        [HttpPut("{id:guid}")]
        public IActionResult Actualizar(Guid id, ProveedorDto dto)
        {
            var resultado = _proveedorService.Modificar(id, dto);

            return resultado switch
            {
                ResultadoProveedor.NoEncontrado => NotFound("No se encontró el proveedor a modificar."),
                ResultadoProveedor.NitDuplicado => Conflict("Ya existe otro proveedor con ese NIT."),
                _ => Ok("Proveedor actualizado correctamente.")
            };
        }

        [HttpGet("{id:guid}/compras")]
        public IActionResult HistorialCompras(Guid id)
        {
            var historial = _proveedorService.ObtenerHistorialCompras(id);
            if (historial == null)
                return NotFound("El proveedor no se encuentra registrado.");
            return Ok(historial);
        }
    }
}