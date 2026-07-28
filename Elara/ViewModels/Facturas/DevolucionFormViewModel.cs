using System.ComponentModel.DataAnnotations;
using ElaraMVC.Models;

namespace ElaraMVC.ViewModels;

public class DevolucionFormViewModel : IValidatableObject
{
    [Required]
    public int FacturaDetalleId { get; set; }

    // Datos de solo lectura para mostrar en el formulario antes de confirmar;
    // el servidor siempre recalcula el monto real desde la línea, nunca de acá.
    public string ProductoNombre { get; set; } = string.Empty;
    public int CantidadFacturada { get; set; }
    public int CantidadYaDevuelta { get; set; }
    public int CantidadDisponible { get; set; }
    public decimal PrecioUnitario { get; set; }

    [Range(1, 10000, ErrorMessage = "La cantidad a devolver debe ser al menos 1.")]
    [Display(Name = "Cantidad a devolver")]
    public int Cantidad { get; set; } = 1;

    [Required(ErrorMessage = "Indica el motivo de la devolución.")]
    [StringLength(300, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Motivo")]
    public string Motivo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecciona el método de reembolso.")]
    [Display(Name = "Método de reembolso")]
    public MetodoPago MetodoReembolso { get; set; } = MetodoPago.Efectivo;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Cantidad > CantidadDisponible && CantidadDisponible > 0)
        {
            yield return new ValidationResult(
                $"Solo quedan {CantidadDisponible} unidad(es) disponibles para devolver de esta línea.",
                new[] { nameof(Cantidad) });
        }
    }
}
