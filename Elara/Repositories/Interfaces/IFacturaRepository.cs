using ElaraMVC.Models;

namespace ElaraMVC.Repositories.Interfaces;

public interface IFacturaRepository
{
    // citaId -> facturaId, para saber sin N+1 qué citas del calendario ya
    // tienen factura generada.
    Task<Dictionary<int, int>> GetFacturaIdsPorCitaAsync(IEnumerable<int> citaIds);
    Task<Factura?> GetByCitaIdAsync(int citaId);
    Task<Factura?> GetByIdAsync(int id);
    Task<List<Factura>> GetEnRangoAsync(DateTime desde, DateTime hasta);
    Task AddAsync(Factura factura);
    void Update(Factura factura);
    Task SaveChangesAsync();
}
