namespace ElaraMVC.Models;

public class HorarioTrabajo
{
    public int Id { get; set; }

    public int EmpleadoId { get; set; }
    public Empleado? Empleado { get; set; }

    public DayOfWeek DiaSemana { get; set; }
    public bool Trabaja { get; set; }
    public TimeSpan? HoraInicio { get; set; }
    public TimeSpan? HoraFin { get; set; }
}
