using ElaraMVC.Models;
using ElaraMVC.ViewModels;
using Microsoft.AspNetCore.Http;

namespace ElaraMVC.Services.Interfaces;

public interface IFacturaService
{
    Task<FacturaFormViewModel?> ConstruirFormularioAsync(int citaId);
    Task<Factura?> ObtenerPorIdAsync(int id);
    Task<(bool Success, string? Error, int? FacturaId)> CrearAsync(FacturaFormViewModel model);
    Task<VentaRapidaViewModel> ConstruirVentaRapidaFormularioAsync();
    Task<(bool Success, string? Error, int? FacturaId)> CrearVentaRapidaAsync(VentaRapidaViewModel model);
    Task<(bool Success, string? Error)> SubirComprobanteTransferenciaAsync(int facturaId, IFormFile archivo);
    Task<ReporteCajaViewModel> ObtenerReporteCajaAsync(DateTime fecha);
    Task<List<ComisionEmpleadoViewModel>> ObtenerReporteComisionesAsync(DateTime desde, DateTime hasta);
}
