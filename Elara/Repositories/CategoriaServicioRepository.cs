using ElaraMVC.Data;
using ElaraMVC.Models;
using ElaraMVC.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ElaraMVC.Repositories;

public class CategoriaServicioRepository : ICategoriaServicioRepository
{
    private readonly ApplicationDbContext _context;

    public CategoriaServicioRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<List<CategoriaServicio>> GetAllAsync()
    {
        return _context.CategoriasServicio.OrderBy(c => c.Nombre).ToListAsync();
    }

    public Task<List<CategoriaServicio>> GetActivasAsync()
    {
        return _context.CategoriasServicio
            .Where(c => c.Activo)
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }

    public Task<CategoriaServicio?> GetByIdAsync(int id)
    {
        return _context.CategoriasServicio.FirstOrDefaultAsync(c => c.Id == id);
    }

    public Task<bool> ExisteNombreAsync(string nombre, int? excluirId)
    {
        return _context.CategoriasServicio
            .AnyAsync(c => c.Nombre == nombre && (excluirId == null || c.Id != excluirId));
    }

    public Task<bool> TieneServiciosAsync(int id)
    {
        return _context.Servicios.AnyAsync(s => s.CategoriaServicioId == id);
    }

    public async Task AddAsync(CategoriaServicio categoria)
    {
        await _context.CategoriasServicio.AddAsync(categoria);
    }

    public void Update(CategoriaServicio categoria)
    {
        _context.CategoriasServicio.Update(categoria);
    }

    public void Remove(CategoriaServicio categoria)
    {
        _context.CategoriasServicio.Remove(categoria);
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
