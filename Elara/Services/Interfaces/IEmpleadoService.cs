using ElaraMVC.Models;
using ElaraMVC.ViewModels;

namespace ElaraMVC.Services.Interfaces;

public interface IEmpleadoService
{
    Task<List<Empleado>> ListarAsync();
    Task<Empleado?> ObtenerPorIdAsync(int id);
    Task<Empleado?> ObtenerPorUsuarioAsync(string applicationUserId);
    Task<EmpleadoFormViewModel> ConstruirFormularioNuevoAsync();
    Task<EmpleadoFormViewModel?> ConstruirFormularioEdicionAsync(int id);
    Task<(bool Success, string? Error, string? PasswordTemporal)> CrearAsync(EmpleadoFormViewModel model);
    Task<(bool Success, string? Error)> ActualizarAsync(EmpleadoFormViewModel model);
    Task<bool> CambiarEstadoAsync(int id, EstadoEmpleado nuevoEstado);
    Task<decimal> CalcularComisionGanadaAsync(int empleadoId, DateTime desde, DateTime hasta);
}
