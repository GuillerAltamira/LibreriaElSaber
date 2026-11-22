using System.Net.Http.Json;
using System.Text.Json;
using Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Proveedores
{
    public class HistorialModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HistorialModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public ProveedorItem? Proveedor { get; set; }
        public List<HistorialCompraDto> Compras { get; set; } = new();

        // Datos para las tarjetas de resumen
        public decimal TotalGeneral => Compras.Sum(c => c.Total);
        public int TotalProductos => Compras.Sum(c => c.Detalles.Sum(d => d.Cantidad));
        public DateTime? UltimaCompra => Compras.Count > 0 ? Compras.Max(c => c.Fecha) : null;
        public decimal PromedioPorCompra => Compras.Count > 0 ? TotalGeneral / Compras.Count : 0;

        // Datos para el gráfico: total comprado por mes (del más antiguo al más nuevo)
        public string MesesJson { get; set; } = "[]";
        public string TotalesPorMesJson { get; set; } = "[]";

        // Producto que más se le compró a este proveedor
        public string? ProductoTop => Compras
            .SelectMany(c => c.Detalles)
            .GroupBy(d => d.Producto)
            .OrderByDescending(g => g.Sum(d => d.Cantidad))
            .Select(g => g.Key)
            .FirstOrDefault();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var client = _httpClientFactory.CreateClient("LibreriaApi");

            var response = await client.GetAsync($"api/Proveedor/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Mensaje"] = "El proveedor no se encuentra registrado.";
                TempData["EsError"] = "1";
                return RedirectToPage("Index");
            }
            Proveedor = await response.Content.ReadFromJsonAsync<ProveedorItem>();

            // Historial de compras: usa el HistorialCompraDto de Contratos
            Compras = await client.GetFromJsonAsync<List<HistorialCompraDto>>($"api/Proveedor/{id}/compras") ?? new();

            var porMes = Compras
                .GroupBy(c => new { c.Fecha.Year, c.Fecha.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .ToList();
            MesesJson = JsonSerializer.Serialize(porMes.Select(g => $"{g.Key.Month:00}/{g.Key.Year}"));
            TotalesPorMesJson = JsonSerializer.Serialize(porMes.Select(g => g.Sum(c => c.Total)));

            return Page();
        }
    }
}