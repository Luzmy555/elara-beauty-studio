using ElaraMVC.Data;
using ElaraMVC.Models;
using ElaraMVC.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ElaraMVC.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly ApplicationDbContext _context;

    public ClienteRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Cliente> Items, int TotalCount)> GetPagedAsync(string? searchTerm, int pageNumber, int pageSize)
    {
        var query = _context.Clientes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(c =>
                EF.Functions.Like(c.NombreCompleto, $"%{term}%") ||
                EF.Functions.Like(c.Telefono, $"%{term}%"));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(c => c.NombreCompleto)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<Cliente>> BuscarActivosAsync(string? searchTerm, int maxResultados = 10)
    {
        var query = _context.Clientes.Where(c => c.Activo);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(c =>
                EF.Functions.Like(c.NombreCompleto, $"%{term}%") ||
                EF.Functions.Like(c.Telefono, $"%{term}%"));
        }

        return await query
            .OrderBy(c => c.NombreCompleto)
            .Take(maxResultados)
            .ToListAsync();
    }

    public Task<Cliente?> GetByIdAsync(int id)
    {
        return _context.Clientes.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AddAsync(Cliente cliente)
    {
        await _context.Clientes.AddAsync(cliente);
    }

    public void Update(Cliente cliente)
    {
        _context.Clientes.Update(cliente);
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeId)
    {
        var normalized = email.Trim().ToLower();
        var query = _context.Clientes.Where(c => c.Email.ToLower() == normalized);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
