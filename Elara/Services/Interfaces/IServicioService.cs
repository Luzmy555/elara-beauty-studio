using ElaraMVC.Models;
using ElaraMVC.ViewModels;

namespace ElaraMVC.Services.Interfaces;

public interface IServicioService
{
    Task<List<Servicio>> ListarAsync();
    Task<Servicio?> ObtenerPorIdAsync(int id);
    Task<(bool Success, string? Error)> CrearAsync(ServicioFormViewModel model);
    Task<(bool Success, string? Error)> ActualizarAsync(ServicioFormViewModel model);
    Task<bool> CambiarEstadoAsync(int id);
}
