using ElaraMVC.ViewModels;

namespace ElaraMVC.Services.Interfaces;

public interface IReporteService
{
    Task<ReporteKpiViewModel> ObtenerKpisAsync();
    Task<List<SerieMensualViewModel>> ObtenerIngresosPorMesAsync(DateTime desde, DateTime hasta);
    Task<List<CategoriaConteoViewModel>> ObtenerServiciosMasSolicitadosAsync(DateTime desde, DateTime hasta);
    Task<List<RankingEmpleadoViewModel>> ObtenerRankingEmpleadosAsync(DateTime desde, DateTime hasta);
    Task<List<SerieMensualViewModel>> ObtenerClientesNuevosPorMesAsync(DateTime desde, DateTime hasta);
    Task<DashboardViewModel> ObtenerDashboardAsync();
}
