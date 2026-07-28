namespace ElaraMVC.Models;

public class Especialidad
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public ICollection<EmpleadoEspecialidad> EmpleadoEspecialidades { get; set; } = new List<EmpleadoEspecialidad>();
}
