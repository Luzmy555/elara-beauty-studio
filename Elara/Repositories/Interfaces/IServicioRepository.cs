using ElaraMVC.Models;

namespace ElaraMVC.Repositories.Interfaces;

public interface IServicioRepository
{
    Task<List<Servicio>> GetAllAsync();
    Task<Servicio?> GetByIdAsync(int id);
    Task AddAsync(Servicio servicio);
    void Update(Servicio servicio);
    Task SaveChangesAsync();
}
