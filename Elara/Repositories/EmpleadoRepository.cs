using ElaraMVC.Data;
using ElaraMVC.Models;
using ElaraMVC.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ElaraMVC.Repositories;

public class EmpleadoRepository : IEmpleadoRepository
{
    private readonly ApplicationDbContext _context;

    public EmpleadoRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<List<Empleado>> GetAllAsync()
    {
        return _context.Empleados
            .Include(e => e.EmpleadoEspecialidades).ThenInclude(ee => ee.Especialidad)
            .Include(e => e.Horarios)
            .OrderBy(e => e.NombreCompleto)
            .ToListAsync();
    }

    public Task<Empleado?> GetByIdAsync(int id)
    {
        return _context.Empleados
            .Include(e => e.EmpleadoEspecialidades).ThenInclude(ee => ee.Especialidad)
            .Include(e => e.Horarios)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public Task<Empleado?> GetByApplicationUserIdAsync(string applicationUserId)
    {
        return _context.Empleados
            .Include(e => e.Horarios)
            .FirstOrDefaultAsync(e => e.ApplicationUserId == applicationUserId);
    }

    public Task<List<Especialidad>> GetEspecialidadesAsync()
    {
        return _context.Especialidades.OrderBy(s => s.Nombre).ToListAsync();
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeId)
    {
        var normalized = email.Trim().ToLower();
        var query = _context.Empleados.Where(e => e.Email.ToLower() == normalized);

        if (excludeId.HasValue)
        {
            query = query.Where(e => e.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task AddAsync(Empleado empleado)
    {
        await _context.Empleados.AddAsync(empleado);
    }

    public void Update(Empleado empleado)
    {
        _context.Empleados.Update(empleado);
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
