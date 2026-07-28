using ElaraMVC.Models;
using ElaraMVC.ViewModels;

namespace ElaraMVC.Services.Interfaces;

public interface IFacturaService
{
    Task<FacturaFormViewModel?> ConstruirFormularioAsync(int citaId);
    Task<Factura?> ObtenerPorIdAsync(int id);
    Task<(bool Success, string? Error, int? FacturaId)> CrearAsync(FacturaFormViewModel model);
    Task<ReporteCajaViewModel> ObtenerReporteCajaAsync(DateTime fecha);
    Task<List<ComisionEmpleadoViewModel>> ObtenerReporteComisionesAsync(DateTime desde, DateTime hasta);
}
