using Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace Web.Pages.Login
{
    public class RegistroModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public RegistroModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public RegistroUsuarioDto RegistroInput { get; set; } = new();

        public string? MensajeError { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            RegistroInput.Rol = "Usuario";

            if (!ModelState.IsValid)
            {
                MensajeError = "Verifica que todos los campos estén completos (contraseña mínimo 4 caracteres).";
                return Page();
            }

            try
            {
                var client = _httpClientFactory.CreateClient("LibreriaApi");
                var json = JsonSerializer.Serialize(RegistroInput);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("api/Auth/registrar", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["MensajeExito"] = "¡Cuenta creada con éxito! Ahora puedes iniciar sesión.";
                    return RedirectToPage("/Login/Index");
                }

                var errorApi = await response.Content.ReadAsStringAsync();
                MensajeError = string.IsNullOrWhiteSpace(errorApi)
                    ? "No se pudo completar el registro."
                    : errorApi.Trim('"');
                return Page();
            }
            catch (Exception)
            {
                MensajeError = "No se pudo conectar con el servidor de la API.";
                return Page();
            }
        }
    }
}
