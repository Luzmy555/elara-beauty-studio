using ElaraMVC.Models;

namespace ElaraMVC.Repositories.Interfaces;

public interface IClienteRepository
{
    Task<(List<Cliente> Items, int TotalCount)> GetPagedAsync(string? searchTerm, int pageNumber, int pageSize);
    Task<List<Cliente>> BuscarActivosAsync(string? searchTerm, int maxResultados = 10);
    Task<Cliente?> GetByIdAsync(int id);
    Task AddAsync(Cliente cliente);
    void Update(Cliente cliente);
    Task<bool> EmailExistsAsync(string email, int? excludeId);
    Task SaveChangesAsync();
}
