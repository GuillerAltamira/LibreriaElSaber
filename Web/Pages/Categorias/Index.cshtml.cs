using System.Net.Http.Json;
using Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Categorias
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<CategoriaDto> Categorias { get; set; } = new();

        [BindProperty]
        public CategoriaDto NuevaCategoria { get; set; } = new();

        public string? Mensaje { get; set; }
        public bool AbrirModal { get; set; }

        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("LibreriaApi");
            var data = await client.GetFromJsonAsync<List<CategoriaDto>>("api/Categoria");
            if (data != null)
                Categorias = data;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(NuevaCategoria.Nombre))
            {
                Mensaje = "El nombre de la categoría es obligatorio.";
                AbrirModal = true;
                await OnGetAsync();
                return Page();
            }

            var client = _httpClientFactory.CreateClient("LibreriaApi");
            var response = await client.PostAsJsonAsync("api/Categoria", new CategoriaDto
            {
                Nombre = NuevaCategoria.Nombre,
                Descripcion = NuevaCategoria.Descripcion
            });

            if (response.IsSuccessStatusCode)
            {
                Mensaje = "Categoría registrada correctamente.";
                NuevaCategoria = new CategoriaDto();
            }
            else
            {
                Mensaje = "No se pudo registrar la categoría.";
                AbrirModal = true;
            }

            await OnGetAsync();
            return Page();
        }
    }
}
