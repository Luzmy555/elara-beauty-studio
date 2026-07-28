using ElaraMVC.Models;
using ElaraMVC.ViewModels;

namespace ElaraMVC.Services.Interfaces;

public interface ICategoriaServicioService
{
    Task<List<CategoriaServicio>> ListarAsync();
    Task<List<CategoriaServicio>> ListarActivasAsync();
    Task<CategoriaServicio?> ObtenerPorIdAsync(int id);
    Task<(bool Success, string? Error)> CrearAsync(CategoriaServicioFormViewModel model);
    Task<(bool Success, string? Error)> ActualizarAsync(CategoriaServicioFormViewModel model);
    Task<bool> CambiarEstadoAsync(int id);
    Task<(bool Success, string? Error)> EliminarAsync(int id);
}
