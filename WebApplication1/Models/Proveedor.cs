using System.Text.Json.Serialization;

namespace WebApplication1.Models
{
    public class Proveedor
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Nit { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public string? Direccion { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaRegistro { get; set; }

        // Un proveedor tiene muchas compras (1 a muchos)
        [JsonIgnore]
        public ICollection<Compra> Compras { get; set; } = new List<Compra>();
    }
}