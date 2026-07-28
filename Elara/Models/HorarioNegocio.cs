namespace ElaraMVC.Models;

public class HorarioNegocio
{
    public int Id { get; set; }

    public int ConfiguracionNegocioId { get; set; }
    public ConfiguracionNegocio? ConfiguracionNegocio { get; set; }

    public DayOfWeek DiaSemana { get; set; }
    public bool Abierto { get; set; }
    public TimeSpan? HoraApertura { get; set; }
    public TimeSpan? HoraCierre { get; set; }
}
