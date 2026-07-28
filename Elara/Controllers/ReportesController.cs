using ClosedXML.Excel;
using ElaraMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElaraMVC.Controllers;

[Authorize(Roles = "Administrador")]
public class ReportesController : Controller
{
    private readonly IReporteService _reporteService;

    public ReportesController(IReporteService reporteService)
    {
        _reporteService = reporteService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Los KPI son siempre "hoy"/"este mes" — no dependen del filtro de
        // fechas del dashboard, que solo aplica a los 4 gráficos.
        var kpis = await _reporteService.ObtenerKpisAsync();
        return View(kpis);
    }

    [HttpGet]
    public async Task<IActionResult> IngresosPorMes(DateTime? desde, DateTime? hasta)
    {
        var (fechaDesde, fechaHasta) = NormalizarRango(desde, hasta);
        var datos = await _reporteService.ObtenerIngresosPorMesAsync(fechaDesde, fechaHasta);
        return Json(datos);
    }

    [HttpGet]
    public async Task<IActionResult> ServiciosMasSolicitados(DateTime? desde, DateTime? hasta)
    {
        var (fechaDesde, fechaHasta) = NormalizarRango(desde, hasta);
        var datos = await _reporteService.ObtenerServiciosMasSolicitadosAsync(fechaDesde, fechaHasta);
        return Json(datos);
    }

    [HttpGet]
    public async Task<IActionResult> RankingEmpleados(DateTime? desde, DateTime? hasta)
    {
        var (fechaDesde, fechaHasta) = NormalizarRango(desde, hasta);
        var datos = await _reporteService.ObtenerRankingEmpleadosAsync(fechaDesde, fechaHasta);
        return Json(datos);
    }

    [HttpGet]
    public async Task<IActionResult> ClientesNuevosPorMes(DateTime? desde, DateTime? hasta)
    {
        var (fechaDesde, fechaHasta) = NormalizarRango(desde, hasta);
        var datos = await _reporteService.ObtenerClientesNuevosPorMesAsync(fechaDesde, fechaHasta);
        return Json(datos);
    }

    [HttpGet]
    public async Task<IActionResult> ExportarIngresosExcel(DateTime? desde, DateTime? hasta)
    {
        var (fechaDesde, fechaHasta) = NormalizarRango(desde, hasta);
        var datos = await _reporteService.ObtenerIngresosPorMesAsync(fechaDesde, fechaHasta);

        using var workbook = new XLWorkbook();
        var hoja = workbook.Worksheets.Add("Ingresos");

        hoja.Cell(1, 1).Value = "Periodo";
        hoja.Cell(1, 2).Value = "Ingresos";
        var encabezado = hoja.Range(1, 1, 1, 2);
        encabezado.Style.Font.Bold = true;
        encabezado.Style.Font.FontColor = XLColor.White;
        encabezado.Style.Fill.BackgroundColor = XLColor.FromHtml("#C9A15A");

        var fila = 2;
        foreach (var item in datos)
        {
            hoja.Cell(fila, 1).Value = item.Etiqueta;
            hoja.Cell(fila, 2).Value = item.Valor;
            hoja.Cell(fila, 2).Style.NumberFormat.Format = "$#,##0.00";
            fila++;
        }

        hoja.Cell(fila, 1).Value = "Total";
        hoja.Cell(fila, 1).Style.Font.Bold = true;
        hoja.Cell(fila, 2).FormulaA1 = $"SUM(B2:B{fila - 1})";
        hoja.Cell(fila, 2).Style.Font.Bold = true;
        hoja.Cell(fila, 2).Style.NumberFormat.Format = "$#,##0.00";

        hoja.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        var nombreArchivo = $"Ingresos_Elara_{fechaDesde:yyyyMMdd}_{fechaHasta:yyyyMMdd}.xlsx";
        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            nombreArchivo);
    }

    // "hasta" se normaliza como límite EXCLUSIVO (medianoche del día
    // siguiente) para incluir el día completo seleccionado en el filtro.
    private static (DateTime Desde, DateTime Hasta) NormalizarRango(DateTime? desde, DateTime? hasta)
    {
        var hoy = DateTime.Today;
        var fechaHasta = (hasta ?? hoy).Date.AddDays(1);

        var baseDesde = (desde ?? hoy.AddMonths(-5)).Date;
        var fechaDesde = new DateTime(baseDesde.Year, baseDesde.Month, 1);

        return (fechaDesde, fechaHasta);
    }
}
