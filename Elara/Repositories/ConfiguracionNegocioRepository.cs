using ElaraMVC.Data;
using ElaraMVC.Models;
using ElaraMVC.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ElaraMVC.Repositories;

public class ConfiguracionNegocioRepository : IConfiguracionNegocioRepository
{
    private readonly ApplicationDbContext _context;

    public ConfiguracionNegocioRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<ConfiguracionNegocio?> ObtenerAsync()
    {
        return _context.ConfiguracionNegocio
            .Include(c => c.Horarios)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(ConfiguracionNegocio configuracion)
    {
        await _context.ConfiguracionNegocio.AddAsync(configuracion);
    }

    public void Update(ConfiguracionNegocio configuracion)
    {
        _context.ConfiguracionNegocio.Update(configuracion);
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
