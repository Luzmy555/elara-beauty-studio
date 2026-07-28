using ElaraMVC.Models;

namespace ElaraMVC.Services.Interfaces;

public interface IDisponibilidadService
{
    // Verifica que el empleado esté activo, dentro de su horario laboral
    // configurado, y sin otra cita que se cruce con [inicio, fin).
    // citaIdExcluir se usa al editar/reagendar para que la propia cita no
    // cuente como un conflicto contra sí misma.
    Task<bool> EstaDisponibleAsync(int empleadoId, DateTime inicio, DateTime fin, int? citaIdExcluir = null);

    Task<List<Empleado>> ObtenerEmpleadosDisponiblesAsync(int servicioId, DateTime inicio, int? citaIdExcluir = null);
}
