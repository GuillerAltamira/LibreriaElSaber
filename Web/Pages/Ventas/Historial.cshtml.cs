using System.Net.Http.Json;
using Contratos;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Ventas
{
    public class HistorialModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HistorialModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<VentaResumenItemDto> Ventas { get; set; } = new();
        public string? MensajeError { get; set; }
        public decimal TotalVendido { get; set; }
        public int TotalArticulosVendidos { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("LibreriaApi");
                var lista = await client.GetFromJsonAsync<List<VentaResumenItemDto>>("api/Venta");
                if (lista != null)
                {
                    Ventas = lista.OrderByDescending(v => v.Fecha).ToList();
                    TotalVendido = Ventas.Sum(v => v.Total);
                    TotalArticulosVendidos = Ventas.Sum(v => v.Detalles?.Sum(d => d.Cantidad) ?? 0);
                }
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al recuperar el historial de ventas: {ex.Message}";
            }
        }
    }
}
