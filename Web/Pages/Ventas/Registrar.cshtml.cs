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

            var request = new VentaDto
            {
                ClienteNombre = ClienteNombre,
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
                    MensajeError = $"No se pudo registrar la venta: {errorData}";
                }
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al procesar la venta: {ex.Message}";
            }

            await CargarProductosAsync();
            return Page();
        }
    }
}
