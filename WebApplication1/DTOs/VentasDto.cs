namespace WebApplication1.DTOs
{
	public class VentaDto
	{
		public string ClienteNombre { get; set; } = string.Empty; 
		public string Nit { get; set; } = string.Empty;
		public string RazonSocial { get; set; } = string.Empty;
		public List<DetalleVentaDto> Detalles { get; set; } = new();
	}

	public class DetalleVentaDto
	{
		public Guid ProductoId { get; set; }
		public int Cantidad { get; set; }
	}
}