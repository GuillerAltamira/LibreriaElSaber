using System.Net.Http.Json;
using Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Ventas
{
    public class FacturaModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public FacturaModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public FacturaImpresaDto? Factura { get; set; }
        public string? MensajeError { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (!id.HasValue || id == Guid.Empty)
            {
                MensajeError = "Identificador de venta no proporcionado.";
                return Page();
            }

            try
            {
                var client = _httpClientFactory.CreateClient("LibreriaApi");
                var factura = await client.GetFromJsonAsync<FacturaImpresaDto>($"api/Venta/{id.Value}/factura");
                if (factura == null)
                {
                    MensajeError = "No se encontró la factura asociada a esta venta.";
                }
                else
                {
                    Factura = factura;
                }
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al obtener la factura: {ex.Message}";
            }

            return Page();
        }
    }
}
