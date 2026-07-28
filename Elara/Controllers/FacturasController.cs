using ElaraMVC.Services.Interfaces;
using ElaraMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElaraMVC.Controllers;

[Authorize(Roles = "Administrador,Recepcionista")]
public class FacturasController : Controller
{
    private readonly IFacturaService _facturaService;

    public FacturasController(IFacturaService facturaService)
    {
        _facturaService = facturaService;
    }

    [HttpGet]
    public async Task<IActionResult> Create(int citaId)
    {
        var model = await _facturaService.ConstruirFormularioAsync(citaId);
        if (model == null)
        {
            TempData["ErrorMessage"] = "No se puede generar la factura: la cita no existe, no está Completada, o ya tiene una factura.";
            return RedirectToAction("Index", "Citas");
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FacturaFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await RecargarDatosSoloLecturaAsync(model);
            return View(model);
        }

        var (success, error, facturaId) = await _facturaService.CrearAsync(model);
        if (!success)
        {
            if (facturaId.HasValue)
            {
                return RedirectToAction(nameof(Details), new { id = facturaId });
            }

            ModelState.AddModelError(string.Empty, error!);
            await RecargarDatosSoloLecturaAsync(model);
            return View(model);
        }

        TempData["SuccessMessage"] = "Factura generada correctamente.";
        return RedirectToAction(nameof(Details), new { id = facturaId });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var factura = await _facturaService.ObtenerPorIdAsync(id);
        if (factura == null)
        {
            return NotFound();
        }

        return View(factura);
    }

    [HttpGet]
    public async Task<IActionResult> Imprimir(int id)
    {
        var factura = await _facturaService.ObtenerPorIdAsync(id);
        if (factura == null)
        {
            return NotFound();
        }

        return View(factura);
    }

    [HttpGet]
    public async Task<IActionResult> ReporteCaja(DateTime? fecha)
    {
        var model = await _facturaService.ObtenerReporteCajaAsync(fecha ?? DateTime.Today);
        return View(model);
    }

    [Authorize(Roles = "Administrador")]
    [HttpGet]
    public async Task<IActionResult> ReporteComisiones(DateTime? desde, DateTime? hasta)
    {
        var fechaDesde = desde ?? DateTime.Today.AddDays(-30);
        var fechaHasta = hasta ?? DateTime.Today;

        var model = await _facturaService.ObtenerReporteComisionesAsync(fechaDesde, fechaHasta);

        ViewBag.Desde = fechaDesde;
        ViewBag.Hasta = fechaHasta;

        return View(model);
    }

    private async Task RecargarDatosSoloLecturaAsync(FacturaFormViewModel model)
    {
        var formulario = await _facturaService.ConstruirFormularioAsync(model.CitaId);
        if (formulario == null)
        {
            return;
        }

        model.ClienteNombre = formulario.ClienteNombre;
        model.EmpleadoNombre = formulario.EmpleadoNombre;
        model.ServicioNombre = formulario.ServicioNombre;
        model.Subtotal = formulario.Subtotal;
    }
}
