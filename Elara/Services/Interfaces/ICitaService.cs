using ElaraMVC.Models;
using ElaraMVC.ViewModels;

namespace ElaraMVC.Services.Interfaces;

public interface ICitaService
{
    Task<List<CitaEventoViewModel>> ObtenerEventosAsync(DateTime desde, DateTime hasta, int? empleadoId = null);
    Task<Cita?> ObtenerPorIdAsync(int id);
    Task<CitaFormViewModel> ConstruirFormularioNuevoAsync(DateTime? fechaSugerida);
    Task<CitaFormViewModel?> ConstruirFormularioEdicionAsync(int id);
    Task<(bool Success, string? Error, int? CitaId)> CrearAsync(CitaFormViewModel model);
    Task<(bool Success, string? Error)> ActualizarAsync(CitaFormViewModel model);
    Task<(bool Success, string? Error)> ReagendarAsync(int citaId, DateTime nuevoInicio);
    Task<(bool Success, string? Error)> CambiarEstadoAsync(int citaId, EstadoCita nuevoEstado, int? empleadoIdSolicitante = null);
    Task<List<Cliente>> BuscarClientesAsync(string term);
}
