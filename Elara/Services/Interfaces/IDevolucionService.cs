using ElaraMVC.Models;
using ElaraMVC.ViewModels;

namespace ElaraMVC.Services.Interfaces;

public interface IDevolucionService
{
    Task<DevolucionFormViewModel?> ConstruirFormularioAsync(int facturaDetalleId);
    Task<(bool Success, string? Error, int? DevolucionId)> CrearAsync(DevolucionFormViewModel model, string usuarioId);
    Task<Devolucion?> ObtenerPorIdAsync(int id);
}
