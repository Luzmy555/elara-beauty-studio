using System.ComponentModel.DataAnnotations;

namespace ElaraMVC.ViewModels;

public class HorarioDiaNegocioViewModel
{
    public DayOfWeek DiaSemana { get; set; }
    public string NombreDia { get; set; } = string.Empty;

    [Display(Name = "Abierto")]
    public bool Abierto { get; set; }

    [DataType(DataType.Time)]
    [Display(Name = "Hora apertura")]
    public TimeSpan? HoraApertura { get; set; }

    [DataType(DataType.Time)]
    [Display(Name = "Hora cierre")]
    public TimeSpan? HoraCierre { get; set; }
}
