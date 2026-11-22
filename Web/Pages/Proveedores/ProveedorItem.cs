namespace Web.Pages.Proveedores
{
    // Lo que devuelve la API al listar/buscar proveedores (incluye Id y fecha)
    public class ProveedorItem
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Nit { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public string? Direccion { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}