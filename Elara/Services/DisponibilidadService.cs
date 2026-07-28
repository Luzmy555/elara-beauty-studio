using ElaraMVC.Data;
using ElaraMVC.Models;
using ElaraMVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ElaraMVC.Services;

// Regla de negocio central del módulo de Citas, aislada del CRUD (CitaService):
// determina si un empleado puede atender un rango de horario dado. Consulta
// directamente el DbContext porque cruza tres agregados (Empleado, Cita,
// Servicio) y forzarlo a través de repositorios individuales fragmentaría
// una sola consulta cohesionada en varias idas y vueltas innecesarias.
public class DisponibilidadService : IDisponibilidadService
{
    private static readonly EstadoCita[] EstadosQueOcupanHorario =
    {
        EstadoCita.Pendiente, EstadoCita.Confirmada, EstadoCita.EnProceso, EstadoCita.Completada
    };

    private readonly ApplicationDbContext _context;

    public DisponibilidadService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> EstaDisponibleAsync(int empleadoId, DateTime inicio, DateTime fin, int? citaIdExcluir = null)
    {
        if (fin <= inicio)
        {
            return false;
        }

        var empleado = await _context.Empleados
            .Include(e => e.Horarios)
            .FirstOrDefaultAsync(e => e.Id == empleadoId);

        if (empleado == null || empleado.Estado != EstadoEmpleado.Activo)
        {
            return false;
        }

        if (!DentroDeHorarioLaboral(empleado, inicio, fin))
        {
            return false;
        }

        var query = _context.Citas.Where(c =>
            c.EmpleadoId == empleadoId &&
            EstadosQueOcupanHorario.Contains(c.Estado) &&
            c.FechaHoraInicio < fin &&
            inicio < c.FechaHoraFin);

        if (citaIdExcluir.HasValue)
        {
            query = query.Where(c => c.Id != citaIdExcluir.Value);
        }

        return !await query.AnyAsync();
    }

    public async Task<List<Empleado>> ObtenerEmpleadosDisponiblesAsync(int servicioId, DateTime inicio, int? citaIdExcluir = null)
    {
        var servicio = await _context.Servicios.FindAsync(servicioId);
        if (servicio == null)
        {
            return new List<Empleado>();
        }

        var fin = inicio.AddMinutes(servicio.DuracionMinutos);

        var empleados = await _context.Empleados
            .Include(e => e.Horarios)
            .Where(e => e.Estado == EstadoEmpleado.Activo)
            .OrderBy(e => e.NombreCompleto)
            .ToListAsync();

        var disponibles = new List<Empleado>();
        foreach (var empleado in empleados)
        {
            if (await EstaDisponibleAsync(empleado.Id, inicio, fin, citaIdExcluir))
            {
                disponibles.Add(empleado);
            }
        }

        return disponibles;
    }

    private static bool DentroDeHorarioLaboral(Empleado empleado, DateTime inicio, DateTime fin)
    {
        if (inicio.Date != fin.Date)
        {
            return false;
        }

        var horario = empleado.Horarios.FirstOrDefault(h => h.DiaSemana == inicio.DayOfWeek);
        if (horario is not { Trabaja: true, HoraInicio: not null, HoraFin: not null })
        {
            return false;
        }

        return inicio.TimeOfDay >= horario.HoraInicio.Value && fin.TimeOfDay <= horario.HoraFin.Value;
    }
}
