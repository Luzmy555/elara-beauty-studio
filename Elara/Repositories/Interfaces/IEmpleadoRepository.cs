using ElaraMVC.Models;

namespace ElaraMVC.Repositories.Interfaces;

public interface IEmpleadoRepository
{
    Task<List<Empleado>> GetAllAsync();
    Task<Empleado?> GetByIdAsync(int id);
    Task<Empleado?> GetByApplicationUserIdAsync(string applicationUserId);
    Task<List<Especialidad>> GetEspecialidadesAsync();
    Task<bool> EmailExistsAsync(string email, int? excludeId);
    Task AddAsync(Empleado empleado);
    void Update(Empleado empleado);
    Task SaveChangesAsync();
}
