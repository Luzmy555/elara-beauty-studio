using ElaraMVC.Data;
using ElaraMVC.Models;
using ElaraMVC.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ElaraMVC.Repositories;

public class FacturaRepository : IFacturaRepository
{
    private readonly ApplicationDbContext _context;

    public FacturaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Dictionary<int, int>> GetFacturaIdsPorCitaAsync(IEnumerable<int> citaIds)
    {
        var ids = citaIds.ToList();
        if (!ids.Any())
        {
            return new Dictionary<int, int>();
        }

        return await _context.Facturas
            .Where(f => f.CitaId.HasValue && ids.Contains(f.CitaId.Value))
            .ToDictionaryAsync(f => f.CitaId!.Value, f => f.Id);
    }

    public Task<Factura?> GetByCitaIdAsync(int citaId)
    {
        return _context.Facturas.FirstOrDefaultAsync(f => f.CitaId == citaId);
    }

    public Task<Factura?> GetByIdAsync(int id)
    {
        return _context.Facturas
            .Include(f => f.Cliente)
            .Include(f => f.FacturaDetalles).ThenInclude(d => d.Servicio)
            .Include(f => f.FacturaDetalles).ThenInclude(d => d.Empleado)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public Task<List<Factura>> GetEnRangoAsync(DateTime desde, DateTime hasta)
    {
        return _context.Facturas
            .Include(f => f.Cliente)
            .Include(f => f.FacturaDetalles).ThenInclude(d => d.Servicio)
            .Include(f => f.FacturaDetalles).ThenInclude(d => d.Empleado)
            .Where(f => f.FechaEmision >= desde && f.FechaEmision < hasta)
            .OrderByDescending(f => f.FechaEmision)
            .ToListAsync();
    }

    public async Task AddAsync(Factura factura)
    {
        await _context.Facturas.AddAsync(factura);
    }

    public void Update(Factura factura)
    {
        _context.Facturas.Update(factura);
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
