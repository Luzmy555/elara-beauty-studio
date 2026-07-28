using System.ComponentModel.DataAnnotations;
using ElaraMVC.Models;
using Microsoft.AspNetCore.Http;

namespace ElaraMVC.ViewModels;

public class EmpleadoFormViewModel : IValidatableObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [StringLength(150, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Nombre completo")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [RegularExpression(@"^\+?[0-9\s\-\(\)]{7,20}$", ErrorMessage = "Formato de teléfono inválido.")]
    [Display(Name = "Teléfono")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Correo inválido.")]
    [StringLength(150)]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [Range(0, 100, ErrorMessage = "La comisión debe estar entre 0 y 100.")]
    [Display(Name = "Comisión por servicio (%)")]
    public decimal ComisionPorcentaje { get; set; } = 40;

    [Display(Name = "Estado")]
    public EstadoEmpleado Estado { get; set; } = EstadoEmpleado.Activo;

    [Display(Name = "Foto de perfil")]
    public IFormFile? Foto { get; set; }
    public string? FotoActualUrl { get; set; }

    public bool EsEdicion { get; set; }

    public List<int> EspecialidadesSeleccionadas { get; set; } = new();
    public List<Especialidad> EspecialidadesDisponibles { get; set; } = new();

    public List<HorarioDiaViewModel> Horarios { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var dia in Horarios)
        {
            if (!dia.Trabaja)
            {
                continue;
            }

            if (dia.HoraInicio == null || dia.HoraFin == null)
            {
                yield return new ValidationResult(
                    $"Define hora de inicio y fin para {dia.NombreDia}.",
                    new[] { nameof(Horarios) });
            }
            else if (dia.HoraInicio >= dia.HoraFin)
            {
                yield return new ValidationResult(
                    $"La hora de inicio debe ser antes que la de fin en {dia.NombreDia}.",
                    new[] { nameof(Horarios) });
            }
        }
    }
}
