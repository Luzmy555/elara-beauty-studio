using ElaraMVC.Models;

namespace ElaraMVC.Repositories.Interfaces;

public interface IProductoRepository
{
    Task<List<Producto>> GetAllAsync(CategoriaProducto? categoria, string? term);
    Task<Producto?> GetByIdAsync(int id);
    Task<List<Producto>> GetBajoStockAsync();
    Task AddAsync(Producto producto);
    void Update(Producto producto);
    Task SaveChangesAsync();
}
