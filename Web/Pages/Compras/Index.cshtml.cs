using System.Net.Http.Json;
using Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.Pages.Compras
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public class ProductoItem
        {
            public Guid Id { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public decimal Precio { get; set; }
            public int Stock { get; set; }
        }

        public List<HistorialCompraDto> Compras { get; set; } = new();
        public List<SelectListItem> OpcionesProveedores { get; set; } = new();
        public List<ProductoItem> ListaProductos { get; set; } = new();

        public decimal TotalInvertido { get; set; }
        public int TotalUnidadesIngresadas { get; set; }

        [BindProperty]
        public CompraDto NuevaCompra { get; set; } = new();

        public string? Mensaje { get; set; }
        public bool EsError { get; set; }
        public bool AbrirModal { get; set; }

        private async Task CargarDatosAsync()
        {
            var client = _httpClientFactory.CreateClient("LibreriaApi");

            Compras = await client.GetFromJsonAsync<List<HistorialCompraDto>>("api/Compra") ?? new();
            TotalInvertido = Compras.Sum(c => c.Total);
            TotalUnidadesIngresadas = Compras.Sum(c => c.CantidadItems);

            var proveedores = await client.GetFromJsonAsync<List<ProveedorDto>>("api/Proveedor") ?? new();
            OpcionesProveedores = proveedores
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Nombre })
                .ToList();

            ListaProductos = await client.GetFromJsonAsync<List<ProductoItem>>("api/Producto") ?? new();
        }

        public async Task OnGetAsync()
        {
            await CargarDatosAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var client = _httpClientFactory.CreateClient("LibreriaApi");
            var response = await client.PostAsJsonAsync("api/Compra", NuevaCompra);

            if (response.IsSuccessStatusCode)
            {
                Mensaje = (await response.Content.ReadAsStringAsync()).Trim('"');
                EsError = false;
                NuevaCompra = new CompraDto();
            }
            else
            {
                Mensaje = (await response.Content.ReadAsStringAsync()).Trim('"');
                EsError = true;
                AbrirModal = true;
            }

            await CargarDatosAsync();
            return Page();
        }
    }
}