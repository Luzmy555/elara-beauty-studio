using ElaraMVC.Models;
using ElaraMVC.Services.Interfaces;
using ElaraMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElaraMVC.Controllers;

[Authorize(Roles = "Administrador")]
public class EmpleadosController : Controller
{
    private readonly IEmpleadoService _empleadoService;

    public EmpleadosController(IEmpleadoService empleadoService)
    {
        _empleadoService = empleadoService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var empleados = await _empleadoService.ListarAsync();
        return View(empleados);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var empleado = await _empleadoService.ObtenerPorIdAsync(id);
        if (empleado == null)
        {
            return NotFound();
        }

        return View(empleado);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = await _empleadoService.ConstruirFormularioNuevoAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmpleadoFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await RecargarEspecialidadesAsync(model);
            return View(model);
        }

        var (success, error, passwordTemporal) = await _empleadoService.CrearAsync(model);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error!);
            await RecargarEspecialidadesAsync(model);
            return View(model);
        }

        TempData["SuccessMessage"] =
            $"Especialista \"{model.NombreCompleto}\" registrado correctamente. " +
            $"Contraseña temporal: {passwordTemporal} — compártela de forma segura; deberá cambiarla al iniciar sesión.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _empleadoService.ConstruirFormularioEdicionAsync(id);
        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EmpleadoFormViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await RecargarEspecialidadesAsync(model);
            return View(model);
        }

        var (success, error) = await _empleadoService.ActualizarAsync(model);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error!);
            await RecargarEspecialidadesAsync(model);
            return View(model);
        }

        TempData["SuccessMessage"] = $"Datos de \"{model.NombreCompleto}\" actualizados correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id, EstadoEmpleado estado)
    {
        await _empleadoService.CambiarEstadoAsync(id, estado);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Comision(int id, DateTime? desde, DateTime? hasta)
    {
        var empleado = await _empleadoService.ObtenerPorIdAsync(id);
        if (empleado == null)
        {
            return NotFound();
        }

        var fechaDesde = desde ?? DateTime.Today.AddDays(-30);
        var fechaHasta = hasta ?? DateTime.Today;

        var total = await _empleadoService.CalcularComisionGanadaAsync(id, fechaDesde, fechaHasta);

        return Json(new
        {
            desde = fechaDesde.ToString("dd/MM/yyyy"),
            hasta = fechaHasta.ToString("dd/MM/yyyy"),
            total = total.ToString("0.00")
        });
    }

    private async Task RecargarEspecialidadesAsync(EmpleadoFormViewModel model)
    {
        var formulario = await _empleadoService.ConstruirFormularioNuevoAsync();
        model.EspecialidadesDisponibles = formulario.EspecialidadesDisponibles;
    }
}
