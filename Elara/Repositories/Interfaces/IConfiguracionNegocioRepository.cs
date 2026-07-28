using ElaraMVC.Models;

namespace ElaraMVC.Repositories.Interfaces;

public interface IConfiguracionNegocioRepository
{
    Task<ConfiguracionNegocio?> ObtenerAsync();
    Task AddAsync(ConfiguracionNegocio configuracion);
    void Update(ConfiguracionNegocio configuracion);
    Task SaveChangesAsync();
}
