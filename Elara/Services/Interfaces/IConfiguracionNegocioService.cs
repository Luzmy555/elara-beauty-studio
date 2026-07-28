using ElaraMVC.Models;
using ElaraMVC.ViewModels;

namespace ElaraMVC.Services.Interfaces;

public interface IConfiguracionNegocioService
{
    Task<ConfiguracionNegocio> ObtenerAsync();
    Task<ConfiguracionNegocioViewModel> ConstruirFormularioAsync();
    Task<(bool Success, string? Error)> ActualizarAsync(ConfiguracionNegocioViewModel model);
}
