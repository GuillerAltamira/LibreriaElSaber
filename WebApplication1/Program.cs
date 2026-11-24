using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using WebApplication1.Data;
using WebApplication1.Factories;
using WebApplication1.Interfaces;
using WebApplication1.Models;
using WebApplication1.Services;

var builder = WebApplication.CreateBuilder(args);

var db = builder.Configuration.GetConnectionString("ConexionDB");

builder.Services.AddDbContext<BaseDatos>(options =>
    options.UseSqlServer(db));

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddScoped<IProductoInterface, ProductoService>();
builder.Services.AddScoped<IProveedorInterface, ProveedorService>();
builder.Services.AddScoped<ICompraInterface, CompraService>();
builder.Services.AddScoped<IVentaInterface, VentaService>();
builder.Services.AddScoped<IUsuarioInterface, UsuarioService>();

// EL GUARDIA: revisa que el token sea válido (firmado con nuestra clave y no vencido)
var clave = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Clave"]!);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(clave)
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();

// Swagger con el botón "Authorize" (candado) para pegar el token
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Pega aquí tu token (sin escribir 'Bearer'). Lo obtienes en POST /api/Auth/login"
    });
    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

var app = builder.Build();

// Base de datos + semillas
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BaseDatos>();

    // Crea la base de datos / aplica las migraciones pendientes (ya no hace falta Update-Database)
    context.Database.Migrate();

    // Usuario administrador por defecto: admin / 123456
    if (!context.usuarios.Any())
    {
        context.usuarios.Add(new Usuario
        {
            Id = Guid.NewGuid(),
            NombreCompleto = "Administrador El Saber",
            NombreUsuario = "admin",
            Password = new Microsoft.AspNetCore.Identity.PasswordHasher<Usuario>().HashPassword(null!, "123456"),   // encriptada
            Rol = "Administrador"
        });
        context.SaveChanges();
    }

    // Mínimo 100 registros por tabla (Factories/SemillasFactory.cs)
    SemillasFactory.Sembrar(context);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();   // 1ro: ¿quién eres? (lee el token)
app.UseAuthorization();    // 2do: ¿puedes pasar?
app.MapControllers();

app.Run();
