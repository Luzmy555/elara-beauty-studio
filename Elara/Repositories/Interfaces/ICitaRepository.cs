using ElaraMVC.Models;

namespace ElaraMVC.Repositories.Interfaces;

public interface ICitaRepository
{
    Task<List<Cita>> GetEnRangoAsync(DateTime desde, DateTime hasta, int? empleadoId = null);
    Task<Cita?> GetByIdAsync(int id);
    Task AddAsync(Cita cita);
    void Update(Cita cita);
    Task SaveChangesAsync();
}
