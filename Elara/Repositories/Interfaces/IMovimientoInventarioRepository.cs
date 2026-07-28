using ElaraMVC.Models;

namespace ElaraMVC.Repositories.Interfaces;

public interface IMovimientoInventarioRepository
{
    Task<List<MovimientoInventario>> GetPorProductoAsync(int productoId);
    Task AddAsync(MovimientoInventario movimiento);
    Task SaveChangesAsync();
}
