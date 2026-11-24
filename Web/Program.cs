using Web.Servicios;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Pega el token JWT en cada pedido a la API (Servicios/TokenHandler.cs)
builder.Services.AddTransient<TokenHandler>();

builder.Services.AddHttpClient("LibreriaApi", client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7161/";
    client.BaseAddress = new Uri(baseUrl);
})
.AddHttpMessageHandler<TokenHandler>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

// PROTECCIÓN: sin sesión (o sin token) -> al Login. Compras, Usuarios, Proveedores y Ventas -> solo Administrador (el Usuario solo ve Categorías y Productos).
// Se revisa aquí (antes de la página), así tampoco se pueden enviar formularios sin login.
app.Use(async (context, next) =>
{
    var ruta = context.Request.Path.Value?.ToLower() ?? "";
    var esPublica = ruta.StartsWith("/login") || ruta.StartsWith("/error") || Path.HasExtension(ruta);

    if (!esPublica)
    {
        var nombre = context.Session.GetString("NombreCompleto");
        var rol = context.Session.GetString("Rol");
        var token = context.Session.GetString("Token");

        if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(token))
        {
            context.Session.Clear();
            context.Response.Redirect("/Login");
            return;
        }

        if (rol != "Administrador" && (ruta.StartsWith("/compras") || ruta.StartsWith("/usuarios") || ruta.StartsWith("/proveedores") || ruta.StartsWith("/ventas")))
        {
            context.Response.Redirect("/Index");
            return;
        }
    }

    await next();
});
app.UseAuthorization();
app.MapRazorPages();

app.Run();
