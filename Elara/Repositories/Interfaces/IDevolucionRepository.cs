using ElaraMVC.Models;

namespace ElaraMVC.Repositories.Interfaces;

public interface IDevolucionRepository
{
    Task<FacturaDetalle?> GetFacturaDetalleConDevolucionesAsync(int facturaDetalleId);
    Task<List<Devolucion>> GetEnRangoAsync(DateTime desde, DateTime hasta);
    Task<Devolucion?> GetByIdAsync(int id);
    Task AddAsync(Devolucion devolucion);
    Task SaveChangesAsync();
}
