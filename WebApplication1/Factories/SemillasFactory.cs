using System.Globalization;
using System.Text;
using Bogus;
using Microsoft.AspNetCore.Identity;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Factories
{
    // Siembra datos de prueba con Bogus: MÍNIMO 100 registros por tabla.
    // Si una tabla ya tiene datos, solo completa lo que falta para llegar a 100 (no borra nada).
    public static class SemillasFactory
    {
        private const int Minimo = 100;

        public static void Sembrar(BaseDatos context)
        {
            Randomizer.Seed = new Random(2026);   // mismos datos cada vez que se siembra

            SembrarCategorias(context);
            SembrarProductos(context);
            SembrarProveedores(context);
            SembrarCompras(context);   // las compras SUBEN el stock
            SembrarVentas(context);    // las ventas BAJAN el stock (y generan su factura)
            SembrarUsuarios(context);  // contraseñas ENCRIPTADAS
        }

        // ---------------- CATEGORÍAS (20 tipos x 5 públicos = 100) ----------------
        private static readonly string[] Tipos =
        {
            "Cuadernos", "Libros", "Lapiceros", "Lápices", "Marcadores", "Carpetas", "Archivadores",
            "Papel", "Cartulinas", "Pinturas", "Pinceles", "Reglas", "Tijeras", "Pegamentos",
            "Mochilas", "Calculadoras", "Agendas", "Diccionarios", "Novelas", "Revistas"
        };
        private static readonly string[] Publicos = { "Escolar", "Universitario", "Oficina", "Arte", "Infantil" };

        private static void SembrarCategorias(BaseDatos context)
        {
            var faltan = Minimo - context.categorias.Count();
            if (faltan <= 0) return;

            var existentes = context.categorias.Select(c => c.Nombre).ToHashSet();
            var nuevas = Tipos
                .SelectMany(t => Publicos.Select(p => $"{t} - {p}"))
                .Where(n => !existentes.Contains(n))
                .Take(faltan)
                .Select(n => new Categoria
                {
                    Id = Guid.NewGuid(),
                    Nombre = n,
                    Descripcion = $"Productos de la línea {n.ToLower()}"
                })
                .ToList();

            context.categorias.AddRange(nuevas);
            context.SaveChanges();
        }

        // ---------------- PRODUCTOS ----------------
        private static readonly string[] Marcas =
        {
            "Rhein", "Faber-Castell", "Pelikan", "Stabilo", "Bic", "Maped", "Norma", "Artesco",
            "Pilot", "Casio", "Totto", "Scribe", "Layconsa", "Paper Mate", "Staedtler"
        };

        private static void SembrarProductos(BaseDatos context)
        {
            var faltan = Minimo - context.productos.Count();
            if (faltan <= 0) return;

            var categorias = context.categorias.ToList();

            var faker = new Faker<Producto>("es")
                .RuleFor(p => p.Id, f => Guid.NewGuid())
                .RuleFor(p => p.CategoriaId, f => f.PickRandom(categorias).Id)
                .RuleFor(p => p.Nombre, (f, p) =>
                {
                    var tipo = categorias.First(c => c.Id == p.CategoriaId).Nombre.Split(" - ")[0];
                    return $"{tipo} {f.PickRandom(Marcas)} {f.Random.AlphaNumeric(3).ToUpper()}";
                })
                .RuleFor(p => p.Descripcion, f => f.Commerce.ProductAdjective() + ", " + f.Commerce.ProductMaterial().ToLower())
                .RuleFor(p => p.Precio, f => Math.Round(f.Random.Decimal(2, 350), 2))
                .RuleFor(p => p.Stock, f => f.Random.Int(0, 20))   // stock inicial; las compras lo aumentan
                .RuleFor(p => p.ImagenUrl, f => null);

            context.productos.AddRange(faker.Generate(faltan));
            context.SaveChanges();
        }

        // ---------------- PROVEEDORES ----------------
        private static void SembrarProveedores(BaseDatos context)
        {
            var faltan = Minimo - context.proveedores.Count();
            if (faltan <= 0) return;

            var nitsUsados = context.proveedores.Select(p => p.Nit).ToHashSet();

            var faker = new Faker<Proveedor>("es")
                .RuleFor(p => p.Id, f => Guid.NewGuid())
                .RuleFor(p => p.Nombre, f => f.Company.CompanyName())
                .RuleFor(p => p.Nit, f =>
                {
                    string nit;
                    do { nit = f.Random.Long(1000000, 9999999999).ToString(); }   // NIT único, solo números
                    while (!nitsUsados.Add(nit));
                    return nit;
                })
                .RuleFor(p => p.Telefono, f => f.Phone.PhoneNumber("7#######"))
                .RuleFor(p => p.Correo, (f, p) => f.Internet.Email(p.Nombre.Split(' ')[0]).ToLower())
                .RuleFor(p => p.Direccion, f => f.Address.StreetAddress())
                .RuleFor(p => p.Activo, f => f.Random.Bool(0.85f))               // 85% activos
                .RuleFor(p => p.FechaRegistro, f => f.Date.Past(2));

            context.proveedores.AddRange(faker.Generate(faltan));
            context.SaveChanges();
        }

        // ---------------- COMPRAS (+ detalle) ----------------
        private static void SembrarCompras(BaseDatos context)
        {
            var faltan = Minimo - context.compras.Count();
            if (faltan <= 0) return;

            var f = new Faker("es");
            var proveedores = context.proveedores.Where(p => p.Activo).ToList();   // solo a ACTIVOS
            var productos = context.productos.ToList();
            var numero = context.compras.Count();

            for (int i = 0; i < faltan; i++)
            {
                var compra = new Compra
                {
                    Id = Guid.NewGuid(),
                    ProveedorId = f.PickRandom(proveedores).Id,
                    Fecha = f.Date.Past(1),
                    NumeroFactura = $"FC-{++numero:D6}"
                };

                foreach (var prod in f.PickRandom(productos, f.Random.Int(1, 4)))
                {
                    var cantidad = f.Random.Int(5, 60);
                    var costo = Math.Round(prod.Precio * f.Random.Decimal(0.55m, 0.8m), 2);   // se compra más barato

                    compra.Detalles.Add(new DetalleCompra
                    {
                        Id = Guid.NewGuid(),
                        ProductoId = prod.Id,
                        Cantidad = cantidad,
                        CostoUnitario = costo,
                        Subtotal = cantidad * costo
                    });

                    prod.Stock += cantidad;   // la compra ingresa al inventario
                }

                compra.Total = compra.Detalles.Sum(d => d.Subtotal);
                context.compras.Add(compra);
            }

            context.SaveChanges();
        }

        // ---------------- VENTAS (+ detalle + factura) ----------------
        private static void SembrarVentas(BaseDatos context)
        {
            var faltan = Minimo - context.ventas.Count();
            if (faltan <= 0) return;

            var f = new Faker("es");
            var productos = context.productos.ToList();
            var numero = context.facturas.Count();

            for (int i = 0; i < faltan; i++)
            {
                var conStock = productos.Where(p => p.Stock > 0).ToList();
                if (conStock.Count == 0) break;   // ya no hay nada que vender

                var venta = new Venta
                {
                    Id = Guid.NewGuid(),
                    Fecha = f.Date.Recent(180),
                    Estado = "Completada"
                };

                foreach (var prod in f.PickRandom(conStock, Math.Min(conStock.Count, f.Random.Int(1, 3))))
                {
                    var cantidad = f.Random.Int(1, Math.Min(5, prod.Stock));   // nunca más de lo que hay

                    venta.Detalles.Add(new DetalleVenta
                    {
                        Id = Guid.NewGuid(),
                        ProductoId = prod.Id,
                        Cantidad = cantidad,
                        PrecioUnitario = prod.Precio,
                        Subtotal = cantidad * prod.Precio
                    });

                    prod.Stock -= cantidad;   // la venta descuenta del inventario
                }

                venta.Total = venta.Detalles.Sum(d => d.Subtotal);

                var esEmpresa = f.Random.Bool(0.3f);
                venta.Factura = new Factura
                {
                    Id = Guid.NewGuid(),
                    VentaId = venta.Id,
                    NumeroFactura = $"FAC-{++numero:D6}",
                    FechaEmision = venta.Fecha,
                    NIT = f.Random.Long(1000000, 99999999).ToString(),
                    RazonSocial = esEmpresa ? f.Company.CompanyName() : f.Name.FullName()
                };

                context.ventas.Add(venta);
            }

            context.SaveChanges();
        }
    
        // ---------------- USUARIOS ----------------
        // Todos los usuarios de prueba tienen la contraseña "123456" (guardada ENCRIPTADA).
        private static void SembrarUsuarios(BaseDatos context)
        {
            var faltan = Minimo - context.usuarios.Count();
            if (faltan <= 0) return;

            // Se encripta una sola vez (encriptar es lento a propósito) y se reutiliza
            var passwordEncriptada = new PasswordHasher<Usuario>().HashPassword(null!, "123456");
            var usados = context.usuarios.Select(u => u.NombreUsuario.ToLower()).ToHashSet();

            var faker = new Faker<Usuario>("es")
                .RuleFor(u => u.Id, f => Guid.NewGuid())
                .RuleFor(u => u.NombreCompleto, f => f.Name.FullName())
                .RuleFor(u => u.NombreUsuario, (f, u) =>
                {
                    // Ej: "Juan Pérez" -> "jperez", y si ya existe "jperez2", "jperez3"...
                    var partes = SinTildes(u.NombreCompleto).ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var baseNombre = partes[0][0] + partes[^1];
                    var nombre = baseNombre;
                    for (int n = 2; !usados.Add(nombre); n++)
                        nombre = baseNombre + n;
                    return nombre;
                })
                .RuleFor(u => u.Password, f => passwordEncriptada)
                .RuleFor(u => u.Rol, f => f.Random.Bool(0.1f) ? "Administrador" : "Usuario");   // 10% admins

            context.usuarios.AddRange(faker.Generate(faltan));
            context.SaveChanges();
        }

        // "Pérez Ñuñez" -> "Perez Nunez" (para armar nombres de usuario sin tildes)
        private static string SinTildes(string texto)
        {
            var sb = new StringBuilder();
            foreach (var c in texto.Normalize(NormalizationForm.FormD))
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark && (char.IsLetter(c) || c == ' '))
                    sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
