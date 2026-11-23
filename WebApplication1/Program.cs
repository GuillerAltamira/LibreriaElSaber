
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Interfaces;
using WebApplication1.Models;
using WebApplication1.Services;

var builder = WebApplication.CreateBuilder(args);

var db = builder.Configuration.GetConnectionString("ConexionDB");

builder.Services.AddDbContext<BaseDatos>(options =>
    options.UseSqlServer(db));

builder.Services.AddControllers();

builder.Services.AddScoped<IProductoInterface, ProductoService>();
builder.Services.AddScoped<IProveedorInterface, ProveedorService>();
builder.Services.AddScoped<ICompraInterface, CompraService>();
builder.Services.AddScoped<IVentaInterface, VentaService>();
builder.Services.AddScoped<IUsuarioInterface, UsuarioService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Sembrado de usuario administrador por defecto si no existen usuarios
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BaseDatos>();
    if (!context.usuarios.Any())
    {
        context.usuarios.Add(new Usuario
        {
            Id = Guid.NewGuid(),
            NombreCompleto = "Administrador El Saber",
            NombreUsuario = "admin",
            Password = "123456",
            Rol = "Administrador"
        });
        context.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
