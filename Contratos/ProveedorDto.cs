namespace Contratos
{
    public class ProveedorDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Nit { get; set; }
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public string? Direccion { get; set; }
        public bool Activo { get; set; } = true;
    }
}