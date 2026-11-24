using System.Net;
using System.Net.Http.Headers;

namespace Web.Servicios
{
    // Se "engancha" al HttpClient "LibreriaApi": antes de cada pedido a la API
    // le pega el token guardado en la sesión (Authorization: Bearer ...).
    // Así ninguna página tiene que acordarse de mandarlo.
    public class TokenHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TokenHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            var token = session?.GetString("Token");

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await base.SendAsync(request, cancellationToken);

            // Token vencido o inválido: se cierra la sesión y en el siguiente clic va al Login
            if (response.StatusCode == HttpStatusCode.Unauthorized && !string.IsNullOrEmpty(token))
            {
                session?.Clear();
            }

            return response;
        }
    }
}
