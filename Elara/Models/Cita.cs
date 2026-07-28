namespace ElaraMVC.Models;

public class Cita
{
    public int Id { get; set; }

    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public int EmpleadoId { get; set; }
    public Empleado? Empleado { get; set; }

    public int ServicioId { get; set; }
    public Servicio? Servicio { get; set; }

    public DateTime FechaHoraInicio { get; set; }

    // Se calcula automáticamente sumando Servicio.DuracionMinutos a FechaHoraInicio;
    // nunca se recibe directamente de un formulario.
    public DateTime FechaHoraFin { get; set; }

    public EstadoCita Estado { get; set; } = EstadoCita.Pendiente;

    public string? Notas { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    // Relación con el módulo de Facturación (pendiente): cuando Estado pasa a
    // Completada, la cita queda lista para generar su factura.
    // public Factura? Factura { get; set; }
}
