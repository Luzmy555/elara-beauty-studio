using ElaraMVC.Data;
using ElaraMVC.Models;
using ElaraMVC.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ElaraMVC.Repositories;

public class ServicioRepository : IServicioRepository
{
    private readonly ApplicationDbContext _context;

    public ServicioRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<List<Servicio>> GetAllAsync()
    {
        return _context.Servicios
            .Include(s => s.CategoriaServicio)
            .OrderBy(s => s.CategoriaServicio!.Nombre)
            .ThenBy(s => s.Nombre)
            .ToListAsync();
    }

    public Task<Servicio?> GetByIdAsync(int id)
    {
        return _context.Servicios
            .Include(s => s.CategoriaServicio)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task AddAsync(Servicio servicio)
    {
        await _context.Servicios.AddAsync(servicio);
    }

    public void Update(Servicio servicio)
    {
        _context.Servicios.Update(servicio);
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
