using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class BaseDatos : DbContext
    {
        public BaseDatos(DbContextOptions<BaseDatos> options) : base(options) { }

        public BaseDatos() { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=LibreriaElSaber;Integrated Security=True;Trust Server Certificate=True");
            }
        }

        public DbSet<Producto> productos { get; set; }
        public DbSet<Categoria> categorias { get; set; }
        public DbSet<Proveedor> proveedores { get; set; }
        public DbSet<Compra> compras { get; set; }
        public DbSet<DetalleCompra> detalleCompras { get; set; }
        public DbSet<Venta> ventas { get; set; }
        public DbSet<DetalleVenta> detallesVenta { get; set; }
        public DbSet<Factura> facturas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // CATEGORIA
            modelBuilder.Entity<Categoria>(entity =>
            {
                entity.ToTable("categorias");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Descripcion).HasMaxLength(250);
            });

            // PRODUCTO
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.Property(p => p.Precio).HasPrecision(18, 2);
                entity.Property(p => p.ImagenUrl).HasMaxLength(500);

                entity.HasOne<Categoria>()
                      .WithMany()
                      .HasForeignKey(p => p.CategoriaId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // PROVEEDOR
            modelBuilder.Entity<Proveedor>(entity =>
            {
                entity.ToTable("proveedores");
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Nit).IsRequired().HasMaxLength(20);
                entity.HasIndex(p => p.Nit).IsUnique();

                entity.Property(p => p.Telefono).HasMaxLength(20);
                entity.Property(p => p.Correo).HasMaxLength(100);
                entity.Property(p => p.Direccion).HasMaxLength(200);

                entity.Property(p => p.Activo).IsRequired();
                entity.Property(p => p.FechaRegistro).HasDefaultValueSql("GETDATE()");
            });

            // COMPRA
            modelBuilder.Entity<Compra>(entity =>
            {
                entity.ToTable("compras");
                entity.HasKey(c => c.Id);

                entity.Property(c => c.NumeroFactura).IsRequired().HasMaxLength(30);
                entity.Property(c => c.Fecha).HasDefaultValueSql("GETDATE()");
                entity.Property(c => c.Total).HasPrecision(18, 2);

                entity.HasOne(c => c.Proveedor)
                      .WithMany(p => p.Compras)
                      .HasForeignKey(c => c.ProveedorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(c => new { c.ProveedorId, c.NumeroFactura }).IsUnique();
                entity.HasIndex(c => c.Fecha);
            });

            // DETALLE DE COMPRA
            modelBuilder.Entity<DetalleCompra>(entity =>
            {
                entity.ToTable("detalle_compras", t =>
                    t.HasCheckConstraint("CK_DetalleCompra_Cantidad", "[Cantidad] > 0"));

                entity.HasKey(d => d.Id);
                entity.Property(d => d.CostoUnitario).HasPrecision(18, 2);
                entity.Property(d => d.Subtotal).HasPrecision(18, 2);

                entity.HasOne(d => d.Compra)
                      .WithMany(c => c.Detalles)
                      .HasForeignKey(d => d.CompraId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Producto)
                      .WithMany()
                      .HasForeignKey(d => d.ProductoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // MODULO DE VENTAS
            modelBuilder.Entity<Venta>(entity =>
            {
                entity.ToTable("ventas");
                entity.HasKey(v => v.Id);
                entity.Property(v => v.Total).HasPrecision(18, 2);
                entity.Property(v => v.Fecha).HasDefaultValueSql("GETDATE()");
            });

            modelBuilder.Entity<DetalleVenta>(entity =>
            {
                entity.ToTable("detalle_ventas");
                entity.HasKey(d => d.Id);
                entity.Property(d => d.PrecioUnitario).HasPrecision(18, 2);
                entity.Property(d => d.Subtotal).HasPrecision(18, 2);

                entity.HasOne(d => d.Producto)
                    .WithMany()
                    .HasForeignKey(d => d.ProductoId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Venta)
                    .WithMany(v => v.Detalles)
                    .HasForeignKey(d => d.VentaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Factura>(entity =>
            {
                entity.ToTable("facturas");
                entity.HasKey(f => f.Id);
                entity.Property(f => f.FechaEmision).HasDefaultValueSql("GETDATE()");

                entity.HasOne(f => f.Venta)
                    .WithOne(v => v.Factura)
                    .HasForeignKey<Factura>(f => f.VentaId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(f => f.VentaId).IsUnique();
            });
        }
    }
}