using ElaraMVC.Data;
using ElaraMVC.Models;
using ElaraMVC.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ElaraMVC.Repositories;

public class DevolucionRepository : IDevolucionRepository
{
    private readonly ApplicationDbContext _context;

    public DevolucionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<FacturaDetalle?> GetFacturaDetalleConDevolucionesAsync(int facturaDetalleId)
    {
        return _context.FacturaDetalles
            .Include(d => d.Producto)
            .Include(d => d.Factura)
            .Include(d => d.Devoluciones)
            .FirstOrDefaultAsync(d => d.Id == facturaDetalleId);
    }

    public Task<List<Devolucion>> GetEnRangoAsync(DateTime desde, DateTime hasta)
    {
        return _context.Devoluciones
            .Include(d => d.FacturaDetalle).ThenInclude(fd => fd!.Producto)
            .Include(d => d.FacturaDetalle).ThenInclude(fd => fd!.Factura)
            .Where(d => d.Fecha >= desde && d.Fecha < hasta)
            .OrderByDescending(d => d.Fecha)
            .ToListAsync();
    }

    public Task<Devolucion?> GetByIdAsync(int id)
    {
        return _context.Devoluciones
            .Include(d => d.FacturaDetalle).ThenInclude(fd => fd!.Producto)
            .Include(d => d.FacturaDetalle).ThenInclude(fd => fd!.Factura)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task AddAsync(Devolucion devolucion)
    {
        await _context.Devoluciones.AddAsync(devolucion);
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
