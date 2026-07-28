using ElaraMVC.Models;

namespace ElaraMVC.Repositories.Interfaces;

public interface ICategoriaServicioRepository
{
    Task<List<CategoriaServicio>> GetAllAsync();
    Task<List<CategoriaServicio>> GetActivasAsync();
    Task<CategoriaServicio?> GetByIdAsync(int id);
    Task<bool> ExisteNombreAsync(string nombre, int? excluirId);
    Task<bool> TieneServiciosAsync(int id);
    Task AddAsync(CategoriaServicio categoria);
    void Update(CategoriaServicio categoria);
    void Remove(CategoriaServicio categoria);
    Task SaveChangesAsync();
}
