using System.Net.Http.Json;
using Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.Pages.Productos
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _environment;

        public IndexModel(IHttpClientFactory httpClientFactory, IWebHostEnvironment environment)
        {
            _httpClientFactory = httpClientFactory;
            _environment = environment;
        }

        public class CategoriaItem
        {
            public Guid Id { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public string? Descripcion { get; set; }
        }

        // Lo que devuelve la API (incluye el Id, necesario para editar y eliminar)
        public class ProductoItem
        {
            public Guid Id { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public string Descripcion { get; set; } = string.Empty;
            public decimal Precio { get; set; }
            public int Stock { get; set; }
            public string? ImagenUrl { get; set; }
            public Guid CategoriaId { get; set; }
        }

        // Solo el Administrador puede agregar, editar o eliminar; el Usuario solo mira
        public bool EsAdmin => HttpContext.Session.GetString("Rol") == "Administrador";

        private IActionResult SoloAdmin()
        {
            TempData["Mensaje"] = "Solo el administrador puede modificar productos.";
            TempData["EsError"] = "1";
            return RedirectToPage();
        }

        public List<ProductoItem> Productos { get; set; } = new();
        public Dictionary<Guid, string> NombresCategorias { get; set; } = new();
        public List<SelectListItem> OpcionesCategorias { get; set; } = new();

        // ----- Modal Nuevo -----
        [BindProperty]
        public ProductoDto NuevoProducto { get; set; } = new();

        [BindProperty]
        public IFormFile? ArchivoImagen { get; set; }

        // ----- Modal Editar -----
        [BindProperty(SupportsGet = true)]
        public Guid? EditarId { get; set; }

        [BindProperty]
        public Guid IdEditar { get; set; }

        [BindProperty]
        public ProductoDto ProductoEditado { get; set; } = new();

        [BindProperty]
        public IFormFile? ArchivoImagenEditar { get; set; }

        public string? Mensaje { get; set; }
        public bool EsError { get; set; }
        public bool AbrirModal { get; set; }          // modal Nuevo
        public bool AbrirModalEditar { get; set; }    // modal Editar

        private async Task CargarDatosAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("LibreriaApi");

                var categorias = await client.GetFromJsonAsync<List<CategoriaItem>>("api/Categoria") ?? new();
                NombresCategorias = categorias.ToDictionary(c => c.Id, c => c.Nombre);
                OpcionesCategorias = categorias
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Nombre })
                    .ToList();

                Productos = await client.GetFromJsonAsync<List<ProductoItem>>("api/Producto") ?? new();
            }
            catch (HttpRequestException)
            {
                Mensaje = "No se pudo conectar con la API. Verifica que WebApplication1 esté corriendo.";
                EsError = true;
            }
        }

        // Guarda la imagen en wwwroot/uploads y devuelve la ruta
        private async Task<string?> GuardarImagenAsync(IFormFile? archivo)
        {
            if (archivo == null || archivo.Length == 0)
                return null;

            var carpetaUploads = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(carpetaUploads);

            var nombreArchivo = $"{Guid.NewGuid()}{Path.GetExtension(archivo.FileName)}";
            using var stream = new FileStream(Path.Combine(carpetaUploads, nombreArchivo), FileMode.Create);
            await archivo.CopyToAsync(stream);

            return $"/uploads/{nombreArchivo}";
        }

        // Revisa solo los campos de UN formulario (la página tiene dos: nuevo y editar)
        private bool FormularioValido(string prefijo, ProductoDto dto)
        {
            var claveCategoria = prefijo + ".CategoriaId";
            if (dto.CategoriaId == Guid.Empty)
            {
                ModelState.Remove(claveCategoria);
                ModelState.AddModelError(claveCategoria, "Selecciona una categoría.");
            }

            return ModelState
                .Where(x => x.Key.StartsWith(prefijo + "."))
                .All(x => x.Value!.ValidationState != ModelValidationState.Invalid);
        }

        // Pone el error que devuelve la API debajo del campo que corresponde
        private void ErrorDeApi(string prefijo, string mensaje)
        {
            if (mensaje.Contains("nombre"))
                ModelState.AddModelError(prefijo + ".Nombre", mensaje);
            else if (mensaje.Contains("categoría"))
                ModelState.AddModelError(prefijo + ".CategoriaId", mensaje);
            else
            {
                Mensaje = mensaje;
                EsError = true;
            }
        }

        public async Task OnGetAsync()
        {
            if (TempData["Mensaje"] is string mensaje)
            {
                Mensaje = mensaje;
                EsError = TempData["EsError"] is string;
            }

            await CargarDatosAsync();

            // Clic en "Editar": llena el modal con los datos del producto
            if (EditarId.HasValue && EsAdmin)
            {
                var prod = Productos.FirstOrDefault(p => p.Id == EditarId.Value);
                if (prod != null)
                {
                    IdEditar = prod.Id;
                    ProductoEditado = new ProductoDto
                    {
                        Nombre = prod.Nombre,
                        Descripcion = prod.Descripcion,
                        Precio = prod.Precio,
                        Stock = prod.Stock,
                        ImagenUrl = prod.ImagenUrl,
                        CategoriaId = prod.CategoriaId
                    };
                    AbrirModalEditar = true;
                }
            }
        }

        // Registrar producto (modal Nuevo)
        public async Task<IActionResult> OnPostAsync()
        {
            if (!EsAdmin) return SoloAdmin();

            if (!FormularioValido(nameof(NuevoProducto), NuevoProducto))
            {
                AbrirModal = true;
                await CargarDatosAsync();
                return Page();
            }

            NuevoProducto.ImagenUrl = await GuardarImagenAsync(ArchivoImagen);

            var client = _httpClientFactory.CreateClient("LibreriaApi");
            var response = await client.PostAsJsonAsync("api/Producto", NuevoProducto);

            if (response.IsSuccessStatusCode)
            {
                TempData["Mensaje"] = $"Producto \"{NuevoProducto.Nombre}\" registrado correctamente.";
                return RedirectToPage();
            }

            ErrorDeApi(nameof(NuevoProducto), (await response.Content.ReadAsStringAsync()).Trim('"'));
            AbrirModal = true;
            await CargarDatosAsync();
            return Page();
        }

        // Modificar producto (modal Editar)
        public async Task<IActionResult> OnPostEditarAsync()
        {
            if (!EsAdmin) return SoloAdmin();

            if (!FormularioValido(nameof(ProductoEditado), ProductoEditado))
            {
                AbrirModalEditar = true;
                await CargarDatosAsync();
                return Page();
            }

            // Si subió una imagen nueva la reemplaza; si no, se queda la que tenía
            var nuevaImagen = await GuardarImagenAsync(ArchivoImagenEditar);
            if (nuevaImagen != null)
                ProductoEditado.ImagenUrl = nuevaImagen;

            var client = _httpClientFactory.CreateClient("LibreriaApi");
            var response = await client.PutAsJsonAsync($"api/Producto/{IdEditar}", ProductoEditado);

            if (response.IsSuccessStatusCode)
            {
                TempData["Mensaje"] = $"Producto \"{ProductoEditado.Nombre}\" actualizado correctamente.";
                return RedirectToPage();
            }

            ErrorDeApi(nameof(ProductoEditado), (await response.Content.ReadAsStringAsync()).Trim('"'));
            AbrirModalEditar = true;
            await CargarDatosAsync();
            return Page();
        }

        // Eliminar producto
        public async Task<IActionResult> OnPostEliminarAsync(Guid id)
        {
            if (!EsAdmin) return SoloAdmin();

            var client = _httpClientFactory.CreateClient("LibreriaApi");
            var response = await client.DeleteAsync($"api/Producto/{id}");

            TempData["Mensaje"] = (await response.Content.ReadAsStringAsync()).Trim('"');
            if (!response.IsSuccessStatusCode)
                TempData["EsError"] = "1";

            return RedirectToPage();
        }
    }
}
