using ElaraMVC.Data;
using ElaraMVC.Models;
using ElaraMVC.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ElaraMVC.Repositories;

public class MovimientoInventarioRepository : IMovimientoInventarioRepository
{
    private readonly ApplicationDbContext _context;

    public MovimientoInventarioRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<List<MovimientoInventario>> GetPorProductoAsync(int productoId)
    {
        return _context.MovimientosInventario
            .Where(m => m.ProductoId == productoId)
            .OrderByDescending(m => m.Fecha)
            .ToListAsync();
    }

    public async Task AddAsync(MovimientoInventario movimiento)
    {
        await _context.MovimientosInventario.AddAsync(movimiento);
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
