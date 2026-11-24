using System.Net.Http.Json;
using System.Text.Json;
using Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Ventas
{
    public class RegistrarModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public RegistrarModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public class ProductoItemViewModel
        {
            public Guid Id { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public string Descripcion { get; set; } = string.Empty;
            public decimal Precio { get; set; }
            public int Stock { get; set; }
            public string? ImagenUrl { get; set; }
        }

        public List<ProductoItemViewModel> ProductosDisponibles { get; set; } = new();

        [BindProperty]
        public string ClienteNombre { get; set; } = string.Empty;

        [BindProperty]
        public string Nit { get; set; } = string.Empty;

        [BindProperty]
        public string RazonSocial { get; set; } = string.Empty;

        [BindProperty]
        public string DetallesJson { get; set; } = "[]";

        public string? MensajeError { get; set; }

        public async Task OnGetAsync()
        {
            await CargarProductosAsync();
        }

        // Llamado desde JavaScript al escribir el NIT/CI: devuelve el nombre si el cliente ya compró antes
        public async Task<IActionResult> OnGetClienteAsync(string nit)
        {
            if (string.IsNullOrWhiteSpace(nit) || !nit.All(char.IsDigit))
                return new JsonResult(new { encontrado = false });

            try
            {
                var client = _httpClientFactory.CreateClient("LibreriaApi");
                var response = await client.GetAsync($"api/Venta/cliente/{nit.Trim()}");
                if (response.IsSuccessStatusCode)
                {
                    using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                    var nombre = json.RootElement.TryGetProperty("razonSocial", out var r) ? r.GetString() : null;
                    return new JsonResult(new { encontrado = nombre != null, razonSocial = nombre });
                }
            }
            catch (HttpRequestException) { }

            return new JsonResult(new { encontrado = false });
        }

        private async Task CargarProductosAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("LibreriaApi");
                var productos = await client.GetFromJsonAsync<List<ProductoItemViewModel>>("api/Producto");
                if (productos != null)
                {
                    // Mostrar productos con stock mayor a cero primero
                    ProductosDisponibles = productos.OrderByDescending(p => p.Stock > 0).ThenBy(p => p.Nombre).ToList();
                }
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al comunicarse con la API de productos: {ex.Message}";
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            List<DetalleVentaDto> detalles = new();
            try
            {
                if (!string.IsNullOrWhiteSpace(DetallesJson))
                {
                    detalles = JsonSerializer.Deserialize<List<DetalleVentaDto>>(DetallesJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new();
                }
            }
            catch
            {
                MensajeError = "El formato del detalle de venta no es válido.";
                await CargarProductosAsync();
                return Page();
            }

            if (!detalles.Any())
            {
                MensajeError = "Debe agregar al menos un producto al detalle de la venta.";
                await CargarProductosAsync();
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Nit))
            {
                MensajeError = "El número de NIT o CI es obligatorio para la factura.";
                await CargarProductosAsync();
                return Page();
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(Nit.Trim(), @"^\d{5,15}$"))
            {
                MensajeError = "El NIT/CI solo puede tener números (de 5 a 15 dígitos).";
                await CargarProductosAsync();
                return Page();
            }

            if (string.IsNullOrWhiteSpace(RazonSocial) || RazonSocial.Count(char.IsLetter) < 3)
            {
                MensajeError = "El Nombre o Razón Social es obligatorio y debe tener al menos 3 letras.";
                await CargarProductosAsync();
                return Page();
            }

            // "Nombre del Comprador" es opcional: si está vacío se usa la Razón Social
            var nombreCliente = string.IsNullOrWhiteSpace(ClienteNombre) ? RazonSocial : ClienteNombre;

            var request = new VentaDto
            {
                ClienteNombre = nombreCliente ?? string.Empty,
                Nit = Nit,
                RazonSocial = string.IsNullOrWhiteSpace(RazonSocial) ? ClienteNombre : RazonSocial,
                Detalles = detalles
            };

            try
            {
                var client = _httpClientFactory.CreateClient("LibreriaApi");
                var response = await client.PostAsJsonAsync("api/Venta", request);

                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadFromJsonAsync<VentaResultadoDto>(new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (resultado != null && resultado.VentaId.HasValue)
                    {
                        return RedirectToPage("/Ventas/Factura", new { id = resultado.VentaId.Value });
                    }

                    return RedirectToPage("/Ventas/Historial");
                }
                else
                {
                    var errorData = await response.Content.ReadAsStringAsync();
                    MensajeError = $"No se pudo registrar la venta: {MensajeLegible(errorData)}";
                }
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al procesar la venta: {ex.Message}";
            }

            await CargarProductosAsync();
            return Page();
        }
    
        // Convierte el error de la API en un texto entendible (en vez del JSON técnico)
        private static string MensajeLegible(string respuesta)
        {
            try
            {
                using var json = JsonDocument.Parse(respuesta);
                if (json.RootElement.ValueKind == JsonValueKind.Object &&
                    json.RootElement.TryGetProperty("errors", out var errores))
                {
                    var mensajes = errores.EnumerateObject()
                        .SelectMany(e => e.Value.EnumerateArray().Select(m => m.GetString()))
                        .Where(m => !string.IsNullOrWhiteSpace(m));
                    return string.Join(" ", mensajes);
                }
                if (json.RootElement.ValueKind == JsonValueKind.Object &&
                    json.RootElement.TryGetProperty("mensaje", out var mensaje))
                    return (mensaje.GetString() ?? respuesta).Replace("Error: ", "");
                if (json.RootElement.ValueKind == JsonValueKind.String)
                    return json.RootElement.GetString() ?? respuesta;
            }
            catch (JsonException)
            {
                // no era JSON: es un texto simple
            }
            return respuesta.Trim('"');
        }
    }
}
