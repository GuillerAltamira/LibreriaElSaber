using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Interfaces;
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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
