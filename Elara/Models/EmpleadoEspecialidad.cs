namespace ElaraMVC.Models;

public class EmpleadoEspecialidad
{
    public int EmpleadoId { get; set; }
    public Empleado? Empleado { get; set; }

    public int EspecialidadId { get; set; }
    public Especialidad? Especialidad { get; set; }
}
