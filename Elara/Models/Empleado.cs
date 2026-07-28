namespace ElaraMVC.Models;

public class Empleado
{
    public int Id { get; set; }

    // FK 1 a 1 con AspNetUsers: el especialista inicia sesión con esta cuenta.
    public string ApplicationUserId { get; set; } = string.Empty;
    public ApplicationUser? ApplicationUser { get; set; }

    public string NombreCompleto { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;

    // Es también el nombre de usuario de su cuenta de Identity; no se edita
    // desde este módulo una vez creada la cuenta.
    public string Email { get; set; } = string.Empty;

    public string? FotoUrl { get; set; }

    public decimal ComisionPorcentaje { get; set; }

    public EstadoEmpleado Estado { get; set; } = EstadoEmpleado.Activo;

    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    public ICollection<EmpleadoEspecialidad> EmpleadoEspecialidades { get; set; } = new List<EmpleadoEspecialidad>();
    public ICollection<HorarioTrabajo> Horarios { get; set; } = new List<HorarioTrabajo>();

    // Relación con el módulo de Citas (pendiente): citas asignadas a este especialista.
    // Cuando exista la entidad Cita con FK a EmpleadoId, agregar aquí:
    // public ICollection<Cita> Citas { get; set; } = new List<Cita>();
}
