using Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace Web.Pages.Usuarios
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<UsuarioResponseDto> ListaUsuarios { get; set; } = new();

        [BindProperty]
        public RegistroUsuarioDto NuevoUsuario { get; set; } = new();

        [TempData]
        public string? Mensaje { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (rol != "Administrador")
            {
                return RedirectToPage("/Index");
            }

            await CargarUsuariosAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostCrearAsync()
        {
            if (HttpContext.Session.GetString("Rol") != "Administrador")
            {
                return RedirectToPage("/Index");
            }

            if (!ModelState.IsValid)
            {
                Mensaje = "Error: Completa todos los campos (contraseña mínimo 4 caracteres).";
                return RedirectToPage();
            }

            var client = _httpClientFactory.CreateClient("LibreriaApi");
            var json = JsonSerializer.Serialize(NuevoUsuario);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/Auth/registrar", content);

            if (response.IsSuccessStatusCode)
            {
                Mensaje = "Usuario creado exitosamente.";
            }
            else
            {
                var err = await response.Content.ReadAsStringAsync();
                Mensaje = string.IsNullOrWhiteSpace(err) ? "Error al crear usuario." : err.Trim('"');
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEliminarAsync(Guid id)
        {
            if (HttpContext.Session.GetString("Rol") != "Administrador")
            {
                return RedirectToPage("/Index");
            }

            var client = _httpClientFactory.CreateClient("LibreriaApi");
            var response = await client.DeleteAsync($"api/Auth/usuarios/{id}");

            Mensaje = response.IsSuccessStatusCode
                ? "Usuario eliminado correctamente."
                : "Error al eliminar el usuario.";

            return RedirectToPage();
        }

        private async Task CargarUsuariosAsync()
        {
            var client = _httpClientFactory.CreateClient("LibreriaApi");
            var response = await client.GetAsync("api/Auth/usuarios");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                ListaUsuarios = JsonSerializer.Deserialize<List<UsuarioResponseDto>>(data, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new();
            }
        }
    }
}
