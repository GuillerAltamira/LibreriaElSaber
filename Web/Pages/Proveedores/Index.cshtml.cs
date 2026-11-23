using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Proveedores
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private const int TamanoPagina = 8;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // ---------- Filtros, orden y página (vienen en la URL) ----------
        [BindProperty(SupportsGet = true)] public string? Buscar { get; set; }
        [BindProperty(SupportsGet = true)] public string? Estado { get; set; }   // "activos" / "inactivos"
        [BindProperty(SupportsGet = true)] public string? Orden { get; set; }    // "nombre", "nit", "compras", "total", "estado"
        [BindProperty(SupportsGet = true)] public string? Dir { get; set; }      // "asc" / "desc"
        [BindProperty(SupportsGet = true)] public int Pagina { get; set; } = 1;
        [BindProperty(SupportsGet = true)] public Guid? EditarId { get; set; }   // abre el modal de editar

        // ---------- Datos ----------
        public List<ProveedorItem> Todos { get; set; } = new();          // todos (para tarjetas y gráficos)
        public List<ProveedorItem> Filtrados { get; set; } = new();      // después del buscador/filtro/orden
        public List<ProveedorItem> Proveedores { get; set; } = new();    // solo la página actual (tabla)
        public Dictionary<Guid, ResumenProveedorDto> Resumen { get; set; } = new();
        public int TotalPaginas { get; set; } = 1;

        // ---------- Tarjetas ----------
        public int TotalActivos => Todos.Count(p => p.Activo);
        public int TotalInactivos => Todos.Count(p => !p.Activo);
        public int NuevosEsteMes => Todos.Count(p =>
            p.FechaRegistro.Month == DateTime.Now.Month && p.FechaRegistro.Year == DateTime.Now.Year);
        public decimal TotalComprado => Resumen.Values.Sum(r => r.TotalComprado);

        // ---------- Gráfico Top 5 ----------
        public List<(string Nombre, decimal Total)> Top5 => Todos
            .Select(p => (p.Nombre, TotalDe(p.Id)))
            .Where(x => x.Item2 > 0)
            .OrderByDescending(x => x.Item2)
            .Take(5)
            .ToList();
        public string Top5NombresJson => JsonSerializer.Serialize(Top5.Select(t => t.Nombre));
        public string Top5TotalesJson => JsonSerializer.Serialize(Top5.Select(t => t.Total));

        // ---------- Formularios de los modales ----------
        [BindProperty] public ProveedorDto NuevoProveedor { get; set; } = new();
        [BindProperty] public ProveedorDto ProveedorEditado { get; set; } = new();
        [BindProperty] public Guid IdEditar { get; set; }

        public string? Mensaje { get; set; }
        public bool EsError { get; set; }
        public string? ModalAbierto { get; set; }   // "nuevo" o "editar"

        // ================= AYUDANTES PARA LA VISTA =================

        public int ComprasDe(Guid id) => Resumen.TryGetValue(id, out var r) ? r.CantidadCompras : 0;
        public decimal TotalDe(Guid id) => Resumen.TryGetValue(id, out var r) ? r.TotalComprado : 0;

        // Color del avatar según el nombre (siempre el mismo color para el mismo proveedor)
        public string ColorAvatar(string nombre) => "av" + (Math.Abs(nombre.Sum(c => c)) % 6);

        // Arma los parámetros de la URL conservando filtros, orden y página
        public Dictionary<string, string> Ruta(int? pagina = null, string? orden = null, Guid? editarId = null)
        {
            var r = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(Buscar)) r["buscar"] = Buscar;
            if (!string.IsNullOrWhiteSpace(Estado)) r["estado"] = Estado;

            var o = orden ?? Orden;
            var d = Dir;
            if (orden != null)   // clic en una columna: si ya estaba ordenada, invierte
                d = (orden == Orden && Dir != "desc") ? "desc" : "asc";

            if (!string.IsNullOrEmpty(o)) r["orden"] = o;
            if (!string.IsNullOrEmpty(d)) r["dir"] = d;
            r["pagina"] = (pagina ?? (orden != null ? 1 : Pagina)).ToString();
            if (editarId.HasValue) r["editarId"] = editarId.Value.ToString();
            return r;
        }

        // Flechita de la columna ordenada
        public string IconoOrden(string columna)
        {
            if (Orden != columna && !(columna == "nombre" && string.IsNullOrEmpty(Orden)))
                return "bi-arrow-down-up opacity-25";
            return Dir == "desc" ? "bi-sort-down" : "bi-sort-up";
        }

        // ================= CARGA DE DATOS =================

        private async Task CargarDatosAsync()
        {
            var client = _httpClientFactory.CreateClient("LibreriaApi");
            try
            {
                Todos = await client.GetFromJsonAsync<List<ProveedorItem>>("api/Proveedor") ?? new();
            }
            catch (HttpRequestException)
            {
                Mensaje = "No se pudo conectar con la API. Verifica que WebApplication1 esté corriendo.";
                EsError = true;
                return;
            }

            try
            {
                var resumen = await client.GetFromJsonAsync<List<ResumenProveedorDto>>("api/Proveedor/resumen-compras") ?? new();
                Resumen = resumen.ToDictionary(r => r.ProveedorId);
            }
            catch (HttpRequestException)
            {
                Resumen = new();   // si falla, la página igual funciona (sin totales)
            }

            // 1) Buscador
            IEnumerable<ProveedorItem> lista = Todos;
            if (!string.IsNullOrWhiteSpace(Buscar))
            {
                var texto = Buscar.Trim();
                lista = lista.Where(p =>
                    p.Nombre.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                    p.Nit.Contains(texto, StringComparison.OrdinalIgnoreCase));
            }

            // 2) Filtro de estado
            if (Estado == "activos") lista = lista.Where(p => p.Activo);
            else if (Estado == "inactivos") lista = lista.Where(p => !p.Activo);

            // 3) Orden
            var desc = Dir == "desc";
            lista = Orden switch
            {
                "nit" => desc ? lista.OrderByDescending(p => p.Nit) : lista.OrderBy(p => p.Nit),
                "compras" => desc ? lista.OrderByDescending(p => ComprasDe(p.Id)) : lista.OrderBy(p => ComprasDe(p.Id)),
                "total" => desc ? lista.OrderByDescending(p => TotalDe(p.Id)) : lista.OrderBy(p => TotalDe(p.Id)),
                "estado" => desc ? lista.OrderByDescending(p => p.Activo) : lista.OrderBy(p => p.Activo),
                _ => desc ? lista.OrderByDescending(p => p.Nombre) : lista.OrderBy(p => p.Nombre)
            };
            Filtrados = lista.ToList();

            // 4) Paginación
            TotalPaginas = Math.Max(1, (int)Math.Ceiling(Filtrados.Count / (double)TamanoPagina));
            Pagina = Math.Clamp(Pagina, 1, TotalPaginas);
            Proveedores = Filtrados.Skip((Pagina - 1) * TamanoPagina).Take(TamanoPagina).ToList();
        }

        // Revisa solo los campos de UN formulario (la página tiene dos: nuevo y editar)
        private bool FormularioValido(string prefijo)
        {
            return ModelState
                .Where(x => x.Key.StartsWith(prefijo + "."))
                .All(x => x.Value!.ValidationState != ModelValidationState.Invalid);
        }

        private object RutaActual() => new { Buscar, Estado, Orden, Dir, Pagina };

        // ================= HANDLERS =================

        public async Task OnGetAsync()
        {
            if (TempData["Mensaje"] is string mensaje)
            {
                Mensaje = mensaje;
                EsError = TempData["EsError"] is string;
            }

            await CargarDatosAsync();

            // Clic en "Editar": llenamos el modal con los datos de ese proveedor
            if (EditarId.HasValue)
            {
                var prov = Todos.FirstOrDefault(p => p.Id == EditarId.Value);
                if (prov != null)
                {
                    IdEditar = prov.Id;
                    ProveedorEditado = new ProveedorDto
                    {
                        Nombre = prov.Nombre,
                        Nit = prov.Nit,
                        Telefono = prov.Telefono,
                        Correo = prov.Correo,
                        Direccion = prov.Direccion,
                        Activo = prov.Activo
                    };
                    ModalAbierto = "editar";
                }
            }
        }

        // Botón "Exportar a Excel": descarga la lista filtrada en CSV
        public async Task<IActionResult> OnGetExportarAsync()
        {
            await CargarDatosAsync();

            static string Celda(string? valor) => "\"" + (valor ?? "").Replace("\"", "\"\"") + "\"";

            var csv = new StringBuilder();
            csv.AppendLine("Nombre;NIT;Teléfono;Correo;Dirección;Estado;Compras;Total comprado (Bs);Fecha de registro");
            foreach (var p in Filtrados)
            {
                csv.AppendLine(string.Join(";",
                    Celda(p.Nombre), Celda(p.Nit), Celda(p.Telefono), Celda(p.Correo), Celda(p.Direccion),
                    p.Activo ? "Activo" : "Inactivo",
                    ComprasDe(p.Id),
                    TotalDe(p.Id).ToString("0.00"),
                    p.FechaRegistro.ToString("dd/MM/yyyy")));
            }

            // El BOM al inicio hace que Excel muestre bien las tildes
            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
            return File(bytes, "text/csv", $"proveedores_{DateTime.Now:yyyyMMdd}.csv");
        }

        // Botón "Guardar" del modal Nuevo
        public async Task<IActionResult> OnPostCrearAsync()
        {
            // Si falta algo, cada error ya sale debajo de su campo
            if (!FormularioValido(nameof(NuevoProveedor)))
                return await MostrarModal("nuevo");

            var client = _httpClientFactory.CreateClient("LibreriaApi");
            var response = await client.PostAsJsonAsync("api/Proveedor", NuevoProveedor);

            if (response.IsSuccessStatusCode)
            {
                TempData["Mensaje"] = $"Proveedor \"{NuevoProveedor.Nombre}\" registrado correctamente.";
                return RedirectToPage(RutaActual());
            }

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                // "Ya existe un proveedor con ese NIT." -> debajo del campo NIT
                ModelState.AddModelError("NuevoProveedor.Nit", await response.Content.ReadAsStringAsync());
                return await MostrarModal("nuevo");
            }

            return await MostrarModal("nuevo", "Error al registrar el proveedor.");
        }

        // Botón "Guardar cambios" del modal Editar
        public async Task<IActionResult> OnPostEditarAsync()
        {
            if (!FormularioValido(nameof(ProveedorEditado)))
                return await MostrarModal("editar");

            var client = _httpClientFactory.CreateClient("LibreriaApi");
            var response = await client.PutAsJsonAsync($"api/Proveedor/{IdEditar}", ProveedorEditado);

            if (response.IsSuccessStatusCode)
            {
                TempData["Mensaje"] = $"Proveedor \"{ProveedorEditado.Nombre}\" actualizado correctamente.";
                return RedirectToPage(RutaActual());
            }

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                // "Ya existe otro proveedor con ese NIT." -> debajo del campo NIT
                ModelState.AddModelError("ProveedorEditado.Nit", await response.Content.ReadAsStringAsync());
                return await MostrarModal("editar");
            }

            var error = response.StatusCode == HttpStatusCode.NotFound
                ? await response.Content.ReadAsStringAsync()
                : "Error al actualizar el proveedor.";
            return await MostrarModal("editar", error);
        }

        // Botón de activar / desactivar de cada fila
        public async Task<IActionResult> OnPostCambiarEstadoAsync(Guid id)
        {
            var client = _httpClientFactory.CreateClient("LibreriaApi");
            var actual = await client.GetFromJsonAsync<ProveedorItem>($"api/Proveedor/{id}");

            if (actual != null)
            {
                var dto = new ProveedorDto
                {
                    Nombre = actual.Nombre,
                    Nit = actual.Nit,
                    Telefono = actual.Telefono,
                    Correo = actual.Correo,
                    Direccion = actual.Direccion,
                    Activo = !actual.Activo            // lo invertimos
                };
                var response = await client.PutAsJsonAsync($"api/Proveedor/{id}", dto);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Mensaje"] = $"Proveedor \"{actual.Nombre}\" {(dto.Activo ? "activado" : "desactivado")}.";
                    return RedirectToPage(RutaActual());
                }
            }

            TempData["Mensaje"] = "No se pudo cambiar el estado del proveedor.";
            TempData["EsError"] = "1";
            return RedirectToPage(RutaActual());
        }

        // Vuelve a mostrar la página con el modal abierto.
        // Los errores de cada campo salen debajo del campo; "mensaje" es solo para errores generales.
        private async Task<IActionResult> MostrarModal(string modal, string? mensaje = null)
        {
            if (mensaje != null)
            {
                Mensaje = mensaje;
                EsError = true;
            }
            ModalAbierto = modal;
            await CargarDatosAsync();
            return Page();
        }
    }
}