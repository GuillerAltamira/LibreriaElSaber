using Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace Web.Pages.Login
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public LoginDto LoginInput { get; set; } = new();

        public string? MensajeError { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnGetLogout()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Login/Index");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                MensajeError = "Por favor, completa tu usuario y contraseña.";
                return Page();
            }

            try
            {
                var client = _httpClientFactory.CreateClient("LibreriaApi");
                var json = JsonSerializer.Serialize(LoginInput);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("api/Auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var usuario = JsonSerializer.Deserialize<UsuarioResponseDto>(responseData, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (usuario != null && !string.IsNullOrEmpty(usuario.Token))
                    {
                        // El token JWT: la Web lo manda sola a la API en cada pedido (TokenHandler)
                        HttpContext.Session.SetString("Token", usuario.Token);
                        HttpContext.Session.SetString("UsuarioId", usuario.Id.ToString());
                        HttpContext.Session.SetString("NombreCompleto", usuario.NombreCompleto);
                        HttpContext.Session.SetString("NombreUsuario", usuario.NombreUsuario);
                        HttpContext.Session.SetString("Rol", usuario.Rol);

                        return RedirectToPage("/Index");
                    }
                }

                MensajeError = "Nombre de usuario o contraseña incorrectos.";
                return Page();
            }
            catch (Exception)
            {
                MensajeError = "No se pudo conectar con el servidor de la API. Verifica que esté ejecutándose.";
                return Page();
            }
        }
    }
}
