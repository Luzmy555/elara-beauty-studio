using ElaraMVC.Data;
using ElaraMVC.Models;
using ElaraMVC.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ElaraMVC.Repositories;

public class CitaRepository : ICitaRepository
{
    private readonly ApplicationDbContext _context;

    public CitaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<List<Cita>> GetEnRangoAsync(DateTime desde, DateTime hasta, int? empleadoId = null)
    {
        var query = _context.Citas
            .Include(c => c.Cliente)
            .Include(c => c.Empleado)
            .Include(c => c.Servicio)
            .Where(c => c.FechaHoraInicio < hasta && desde < c.FechaHoraFin);

        if (empleadoId.HasValue)
        {
            query = query.Where(c => c.EmpleadoId == empleadoId.Value);
        }

        return query.OrderBy(c => c.FechaHoraInicio).ToListAsync();
    }

    public Task<Cita?> GetByIdAsync(int id)
    {
        return _context.Citas
            .Include(c => c.Cliente)
            .Include(c => c.Empleado)
            .Include(c => c.Servicio)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AddAsync(Cita cita)
    {
        await _context.Citas.AddAsync(cita);
    }

    public void Update(Cita cita)
    {
        _context.Citas.Update(cita);
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
