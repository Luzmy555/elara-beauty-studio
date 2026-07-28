using System.ComponentModel.DataAnnotations;

namespace ElaraMVC.ViewModels;

public class HorarioDiaViewModel
{
    public DayOfWeek DiaSemana { get; set; }
    public string NombreDia { get; set; } = string.Empty;

    [Display(Name = "Trabaja")]
    public bool Trabaja { get; set; }

    [DataType(DataType.Time)]
    [Display(Name = "Hora inicio")]
    public TimeSpan? HoraInicio { get; set; }

    [DataType(DataType.Time)]
    [Display(Name = "Hora fin")]
    public TimeSpan? HoraFin { get; set; }
}
