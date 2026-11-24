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

        // Solo el Administrador puede agregar; el Usuario solo mira
        public bool EsAdmin => HttpContext.Session.GetString("Rol") == "Administrador";

        public List<CategoriaDto> Categorias { get; set; } = new();

        [BindProperty]
        public CategoriaDto NuevaCategoria { get; set; } = new();

        public string? Mensaje { get; set; }
        public bool AbrirModal { get; set; }

        public async Task OnGetAsync()
        {
            if (TempData["Mensaje"] is string mensaje)
                Mensaje = mensaje;

            try
            {
                var client = _httpClientFactory.CreateClient("LibreriaApi");
                var data = await client.GetFromJsonAsync<List<CategoriaDto>>("api/Categoria");
                if (data != null)
                    Categorias = data;
            }
            catch (HttpRequestException)
            {
                Mensaje = "No se pudo conectar con la API. Verifica que WebApplication1 esté corriendo.";
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!EsAdmin)
            {
                TempData["Mensaje"] = "Solo el administrador puede agregar categorías.";
                return RedirectToPage();
            }

            // Required / StringLength del CategoriaDto: el error sale debajo de cada campo
            if (!ModelState.IsValid)
            {
                AbrirModal = true;
                await OnGetAsync();
                return Page();
            }

            var client = _httpClientFactory.CreateClient("LibreriaApi");
            var response = await client.PostAsJsonAsync("api/Categoria", NuevaCategoria);

            if (response.IsSuccessStatusCode)
            {
                TempData["Mensaje"] = $"Categoría \"{NuevaCategoria.Nombre.Trim()}\" registrada correctamente.";
                return RedirectToPage();
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                // "Ya existe una categoría con ese nombre." -> debajo del campo Nombre
                ModelState.AddModelError("NuevaCategoria.Nombre", await response.Content.ReadAsStringAsync());
            }
            else
            {
                Mensaje = "No se pudo registrar la categoría.";
            }

            AbrirModal = true;
            await OnGetAsync();
            return Page();
        }
    }
}
