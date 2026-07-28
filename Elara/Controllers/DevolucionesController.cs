using ElaraMVC.Models;
using ElaraMVC.Services.Interfaces;
using ElaraMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ElaraMVC.Controllers;

[Authorize(Roles = "Administrador,Recepcionista")]
public class DevolucionesController : Controller
{
    private readonly IDevolucionService _devolucionService;
    private readonly UserManager<ApplicationUser> _userManager;

    public DevolucionesController(IDevolucionService devolucionService, UserManager<ApplicationUser> userManager)
    {
        _devolucionService = devolucionService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Crear(int facturaDetalleId)
    {
        var model = await _devolucionService.ConstruirFormularioAsync(facturaDetalleId);
        if (model == null)
        {
            TempData["ErrorMessage"] = "Esa línea no existe o no corresponde a un producto.";
            return RedirectToAction("ReporteCaja", "Facturas");
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(DevolucionFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await RecargarSoloLecturaAsync(model);
            return View(model);
        }

        var usuarioId = _userManager.GetUserId(User)!;
        var (success, error, devolucionId) = await _devolucionService.CrearAsync(model, usuarioId);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error!);
            await RecargarSoloLecturaAsync(model);
            return View(model);
        }

        TempData["SuccessMessage"] = "Devolución procesada correctamente.";
        return RedirectToAction(nameof(Imprimir), new { id = devolucionId });
    }

    [HttpGet]
    public async Task<IActionResult> Imprimir(int id)
    {
        var devolucion = await _devolucionService.ObtenerPorIdAsync(id);
        if (devolucion == null)
        {
            return NotFound();
        }

        return View(devolucion);
    }

    private async Task RecargarSoloLecturaAsync(DevolucionFormViewModel model)
    {
        var formulario = await _devolucionService.ConstruirFormularioAsync(model.FacturaDetalleId);
        if (formulario == null)
        {
            return;
        }

        model.ProductoNombre = formulario.ProductoNombre;
        model.CantidadFacturada = formulario.CantidadFacturada;
        model.CantidadYaDevuelta = formulario.CantidadYaDevuelta;
        model.CantidadDisponible = formulario.CantidadDisponible;
        model.PrecioUnitario = formulario.PrecioUnitario;
    }
}
