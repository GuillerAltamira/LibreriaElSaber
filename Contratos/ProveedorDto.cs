using System.ComponentModel.DataAnnotations;

namespace Contratos
{
    // IValidatableObject: permite una regla que mira VARIOS campos a la vez
    public class ProveedorDto : IValidatableObject
    {
        // Lo usa el módulo de Compras para llenar la lista de proveedores
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El NIT es obligatorio")]
        [StringLength(20, ErrorMessage = "Máximo 20 caracteres")]
        [RegularExpression(@"^\d{5,20}$", ErrorMessage = "El NIT debe tener solo números (mínimo 5)")]
        public string Nit { get; set; } = string.Empty;

        [StringLength(20)]
        [RegularExpression(@"^[0-9+\s-]{7,20}$", ErrorMessage = "Teléfono no válido (solo números)")]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "Correo no válido")]
        [StringLength(100)]
        public string? Correo { get; set; }

        [StringLength(200)]
        public string? Direccion { get; set; }

        public bool Activo { get; set; } = true;

        // Regla: al menos un dato de contacto (teléfono o correo).
        // Se revisa en la Web y también en la API, porque las dos usan este DTO.
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Telefono) && string.IsNullOrWhiteSpace(Correo))
            {
                yield return new ValidationResult(
                    "Ingresa al menos un teléfono o un correo.",
                    new[] { nameof(Telefono), nameof(Correo) });
            }
        }
    }
}