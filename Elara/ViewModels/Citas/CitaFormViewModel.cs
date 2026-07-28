using System.ComponentModel.DataAnnotations;
using ElaraMVC.Models;

namespace ElaraMVC.ViewModels;

public class CitaFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Selecciona un cliente.")]
    [Display(Name = "Cliente")]
    public int ClienteId { get; set; }

    // Solo para mostrar el nombre en el campo de búsqueda al editar; no se valida.
    public string? ClienteNombre { get; set; }

    [Required(ErrorMessage = "Selecciona un servicio.")]
    [Display(Name = "Servicio")]
    public int ServicioId { get; set; }

    [Required(ErrorMessage = "Selecciona un especialista disponible.")]
    [Display(Name = "Especialista")]
    public int EmpleadoId { get; set; }

    [Required(ErrorMessage = "Selecciona fecha y hora.")]
    [Display(Name = "Fecha y hora")]
    public DateTime FechaHoraInicio { get; set; }

    [StringLength(500, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Notas")]
    public string? Notas { get; set; }

    [Display(Name = "Estado")]
    public EstadoCita Estado { get; set; } = EstadoCita.Pendiente;

    public bool EsEdicion { get; set; }

    public List<Servicio> ServiciosDisponibles { get; set; } = new();
}
