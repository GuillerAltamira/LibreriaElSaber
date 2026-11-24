using System;
using System.Collections.Generic;

namespace Contratos
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

    public class VentaResultadoDto
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public Guid? VentaId { get; set; }
        public string? NumeroFactura { get; set; }
    }

    public class VentaResumenItemDto
    {
        public Guid Id { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; } = string.Empty;
        public FacturaSimpleDto? Factura { get; set; }
        public List<DetalleVentaSimpleDto> Detalles { get; set; } = new();
    }

    public class FacturaSimpleDto
    {
        public string NumeroFactura { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public string Nit { get; set; } = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;
    }

    public class DetalleVentaSimpleDto
    {
        public Guid ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public ProductoDto? Producto { get; set; }
    }
}
