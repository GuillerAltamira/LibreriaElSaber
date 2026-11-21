using System.Net.Http.Json;
using Contratos;
using Microsoft.AspNetCore.Mvc;
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

        public List<ProductoDto> Productos { get; set; } = new();
        public Dictionary<Guid, string> NombresCategorias { get; set; } = new();
        public List<SelectListItem> OpcionesCategorias { get; set; } = new();

        [BindProperty]
        public ProductoDto NuevoProducto { get; set; } = new();

        [BindProperty]
        public IFormFile? ArchivoImagen { get; set; }

        public string? Mensaje { get; set; }
        public bool AbrirModal { get; set; }

        private async Task CargarDatosAsync()
        {
            var client = _httpClientFactory.CreateClient("LibreriaApi");

            var categorias = await client.GetFromJsonAsync<List<CategoriaItem>>("api/Categoria") ?? new();
            NombresCategorias = categorias.ToDictionary(c => c.Id, c => c.Nombre);

            OpcionesCategorias = categorias
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Nombre
                })
                .ToList();

            var productos = await client.GetFromJsonAsync<List<ProductoDto>>("api/Producto") ?? new();
            Productos = productos;
        }

        public async Task OnGetAsync()
        {
            await CargarDatosAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ArchivoImagen != null && ArchivoImagen.Length > 0)
            {
                var carpetaUploads = Path.Combine(_environment.WebRootPath, "uploads");
                Directory.CreateDirectory(carpetaUploads);

                var extension = Path.GetExtension(ArchivoImagen.FileName);
                var nombreArchivo = $"{Guid.NewGuid()}{extension}";
                var rutaCompleta = Path.Combine(carpetaUploads, nombreArchivo);

                using var stream = new FileStream(rutaCompleta, FileMode.Create);
                await ArchivoImagen.CopyToAsync(stream);

                NuevoProducto.ImagenUrl = $"/uploads/{nombreArchivo}";
            }

            var client = _httpClientFactory.CreateClient("LibreriaApi");
            var response = await client.PostAsJsonAsync("api/Producto", NuevoProducto);

            if (response.IsSuccessStatusCode)
            {
                Mensaje = "Producto registrado correctamente.";
                NuevoProducto = new ProductoDto();
                ArchivoImagen = null;
            }
            else
            {
                Mensaje = "Error al registrar el producto. Verifica los campos y la categoría.";
                AbrirModal = true;
            }

            await CargarDatosAsync();
            return Page();
        }
    }
}
