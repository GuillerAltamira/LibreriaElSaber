using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class BaseDatos : DbContext
    {
        public BaseDatos(DbContextOptions<BaseDatos> options) : base(options) { }

        public DbSet<Producto> productos { get; set; }
        public DbSet<Proveedor> proveedores { get; set; }
        public DbSet<Compra> compras { get; set; }
        public DbSet<DetalleCompra> detalleCompras { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== PROVEEDOR =====
            modelBuilder.Entity<Proveedor>(entity =>
            {
                entity.ToTable("proveedores");
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Nit).IsRequired().HasMaxLength(20);
                entity.HasIndex(p => p.Nit).IsUnique();                 // UNIQUE

                entity.Property(p => p.Telefono).HasMaxLength(20);      // NULL permitido
                entity.Property(p => p.Correo).HasMaxLength(100);
                entity.Property(p => p.Direccion).HasMaxLength(200);

                entity.Property(p => p.Activo).IsRequired();
                entity.Property(p => p.FechaRegistro).HasDefaultValueSql("GETDATE()"); // valor por defecto
            });

            // ===== COMPRA =====
            modelBuilder.Entity<Compra>(entity =>
            {
                entity.ToTable("compras");
                entity.HasKey(c => c.Id);

                entity.Property(c => c.NumeroFactura).IsRequired().HasMaxLength(30);
                entity.Property(c => c.Fecha).HasDefaultValueSql("GETDATE()");
                entity.Property(c => c.Total).HasPrecision(18, 2);

                // Proveedor 1 --- * Compras
                entity.HasOne(c => c.Proveedor)
                      .WithMany(p => p.Compras)
                      .HasForeignKey(c => c.ProveedorId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Un proveedor no puede repetir número de factura
                entity.HasIndex(c => new { c.ProveedorId, c.NumeroFactura }).IsUnique();
                entity.HasIndex(c => c.Fecha);
            });

            // ===== DETALLE COMPRA (resuelve Compra * --- * Producto) =====
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
        }
    }
}