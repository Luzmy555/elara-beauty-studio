using ElaraMVC.Models;
using ElaraMVC.ViewModels;

namespace ElaraMVC.Services.Interfaces;

public interface IClienteService
{
    Task<ClienteIndexViewModel> BuscarAsync(string? term, int page);
    Task<Cliente?> ObtenerPorIdAsync(int id);
    Task<(bool Success, string? Error)> CrearAsync(ClienteFormViewModel model);
    Task<(bool Success, string? Error)> ActualizarAsync(ClienteFormViewModel model);
    Task<bool> CambiarEstadoAsync(int id);
}
