using System.ComponentModel.DataAnnotations;
using ElaraMVC.Models;
using Microsoft.AspNetCore.Http;

namespace ElaraMVC.ViewModels;

public class ConfiguracionNegocioViewModel : IValidatableObject
{
    [Required(ErrorMessage = "El nombre del salón es obligatorio.")]
    [StringLength(100, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Nombre del salón")]
    public string NombreSalon { get; set; } = "Elara";

    [Display(Name = "Logo")]
    public IFormFile? Logo { get; set; }
    public string? LogoActualUrl { get; set; }

    [StringLength(200, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Dirección")]
    public string? Direccion { get; set; }

    [RegularExpression(@"^\+?[0-9\s\-\(\)]{7,20}$", ErrorMessage = "Formato de teléfono inválido.")]
    [Display(Name = "Teléfono")]
    public string? Telefono { get; set; }

    [EmailAddress(ErrorMessage = "Correo inválido.")]
    [StringLength(150)]
    [Display(Name = "Email de contacto")]
    public string? EmailContacto { get; set; }

    [Display(Name = "Moneda del sistema")]
    public Moneda Moneda { get; set; } = Moneda.DOP;

    public List<HorarioDiaNegocioViewModel> Horarios { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var dia in Horarios)
        {
            if (!dia.Abierto)
            {
                continue;
            }

            if (dia.HoraApertura == null || dia.HoraCierre == null)
            {
                yield return new ValidationResult(
                    $"Define hora de apertura y cierre para {dia.NombreDia}.",
                    new[] { nameof(Horarios) });
            }
            else if (dia.HoraApertura >= dia.HoraCierre)
            {
                yield return new ValidationResult(
                    $"La hora de apertura debe ser antes que la de cierre en {dia.NombreDia}.",
                    new[] { nameof(Horarios) });
            }
        }
    }
}
