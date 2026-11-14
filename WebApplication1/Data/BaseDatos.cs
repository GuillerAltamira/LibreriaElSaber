using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class BaseDatos : DbContext
    {
        public BaseDatos(DbContextOptions<BaseDatos> options) : base(options) { }

        public DbSet<Producto> productos { get; set; }
    }
}