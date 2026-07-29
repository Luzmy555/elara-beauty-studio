using ElaraMVC.Models;

namespace ElaraMVC.Services.Interfaces;

public interface IDisponibilidadService
{
    // Verifica que el empleado esté activo, dentro de su horario laboral
    // configurado, y sin otra cita que se cruce con [inicio, fin).
    // citaIdExcluir se usa al editar/reagendar para que la propia cita no
    // cuente como un conflicto contra sí misma.
    Task<bool> EstaDisponibleAsync(int empleadoId, DateTime inicio, DateTime fin, int? citaIdExcluir = null);

    // Enumera las horas de inicio candidatas (cada IntervaloMinutos) dentro del
    // horario laboral del empleado ese día, filtrando las que ya están ocupadas
    // o que caen antes de la hora actual si la fecha es hoy.
    Task<List<TimeSpan>> ObtenerHorariosDisponiblesAsync(int empleadoId, int servicioId, DateTime fecha, int? citaIdExcluir = null);
}
