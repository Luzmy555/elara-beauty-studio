using ElaraMVC.Data;
using ElaraMVC.Models;
using ElaraMVC.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ElaraMVC.Repositories;

public class ProductoRepository : IProductoRepository
{
    private readonly ApplicationDbContext _context;

    public ProductoRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Producto>> GetAllAsync(CategoriaProducto? categoria, string? term)
    {
        var query = _context.Productos.AsQueryable();

        if (categoria.HasValue)
        {
            query = query.Where(p => p.Categoria == categoria.Value);
        }

        if (!string.IsNullOrWhiteSpace(term))
        {
            var valor = term.Trim();
            query = query.Where(p =>
                EF.Functions.Like(p.Nombre, $"%{valor}%") ||
                EF.Functions.Like(p.Marca, $"%{valor}%"));
        }

        return await query.OrderBy(p => p.Nombre).ToListAsync();
    }

    public Task<Producto?> GetByIdAsync(int id)
    {
        return _context.Productos.FirstOrDefaultAsync(p => p.Id == id);
    }

    public Task<List<Producto>> GetBajoStockAsync()
    {
        return _context.Productos
            .Where(p => p.CantidadActual < p.CantidadMinima)
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    public async Task AddAsync(Producto producto)
    {
        await _context.Productos.AddAsync(producto);
    }

    public void Update(Producto producto)
    {
        _context.Productos.Update(producto);
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
