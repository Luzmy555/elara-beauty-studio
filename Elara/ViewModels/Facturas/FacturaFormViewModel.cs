using System.ComponentModel.DataAnnotations;
using ElaraMVC.Models;

namespace ElaraMVC.ViewModels;

public class FacturaFormViewModel : IValidatableObject
{
    [Required]
    public int CitaId { get; set; }

    // Solo informativos: el servidor siempre recalcula el Subtotal real a
    // partir de la Cita/Servicio en CrearAsync, nunca confía en este valor.
    public string ClienteNombre { get; set; } = string.Empty;
    public string EmpleadoNombre { get; set; } = string.Empty;
    public string ServicioNombre { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }

    [Range(0, 1000000, ErrorMessage = "El descuento no puede ser negativo.")]
    [Display(Name = "Descuento")]
    public decimal Descuento { get; set; }

    [StringLength(300, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Justificación del descuento")]
    public string? DescuentoJustificacion { get; set; }

    [Required(ErrorMessage = "Selecciona un método de pago.")]
    [Display(Name = "Método de pago")]
    public MetodoPago MetodoPago { get; set; } = MetodoPago.Efectivo;

    [Display(Name = "Estado")]
    public EstadoFactura Estado { get; set; } = EstadoFactura.Pagada;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Descuento > 0 && string.IsNullOrWhiteSpace(DescuentoJustificacion))
        {
            yield return new ValidationResult(
                "Si aplicas un descuento, indica la justificación.",
                new[] { nameof(DescuentoJustificacion) });
        }

        if (Descuento > Subtotal)
        {
            yield return new ValidationResult(
                "El descuento no puede ser mayor al subtotal.",
                new[] { nameof(Descuento) });
        }
    }
}
