using ElaraMVC.Models;
using ElaraMVC.Services.Interfaces;
using ElaraMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElaraMVC.Controllers;

[Authorize(Roles = "Administrador,Recepcionista")]
public class CitasController : Controller
{
    private readonly ICitaService _citaService;
    private readonly IDisponibilidadService _disponibilidadService;

    public CitasController(ICitaService citaService, IDisponibilidadService disponibilidadService)
    {
        _citaService = citaService;
        _disponibilidadService = disponibilidadService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Eventos(DateTime start, DateTime end)
    {
        var eventos = await _citaService.ObtenerEventosAsync(start, end);
        return Json(eventos);
    }

    [HttpGet]
    public async Task<IActionResult> BuscarClientes(string term)
    {
        var clientes = await _citaService.BuscarClientesAsync(term);
        return Json(clientes.Select(c => new { id = c.Id, nombreCompleto = c.NombreCompleto, telefono = c.Telefono }));
    }

    [HttpGet]
    public async Task<IActionResult> HorariosDisponibles(int empleadoId, int servicioId, DateTime fecha, int? citaId)
    {
        if (empleadoId <= 0 || servicioId <= 0 || fecha == default)
        {
            return Json(Array.Empty<object>());
        }

        var horarios = await _disponibilidadService.ObtenerHorariosDisponiblesAsync(empleadoId, servicioId, fecha, citaId);
        return Json(horarios.Select(h => h.ToString(@"hh\:mm")));
    }

    [HttpGet]
    public async Task<IActionResult> FormularioCreacion(DateTime? fecha)
    {
        var model = await _citaService.ConstruirFormularioNuevoAsync(fecha);
        return PartialView("_CitaForm", model);
    }

    [HttpGet]
    public async Task<IActionResult> FormularioEdicion(int id)
    {
        var model = await _citaService.ConstruirFormularioEdicionAsync(id);
        if (model == null)
        {
            return NotFound();
        }

        return PartialView("_CitaForm", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Guardar(CitaFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, error = string.Join(" ", errores) });
        }

        if (model.EsEdicion)
        {
            var (success, error) = await _citaService.ActualizarAsync(model);
            return Json(new { success, error });
        }

        var (creado, errorCreacion, citaId) = await _citaService.CrearAsync(model);
        return Json(new { success = creado, error = errorCreacion, citaId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reagendar(int id, DateTime inicio)
    {
        var (success, error) = await _citaService.ReagendarAsync(id, inicio);
        return Json(new { success, error });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id, EstadoCita estado)
    {
        var (success, error) = await _citaService.CambiarEstadoAsync(id, estado);
        return Json(new { success, error });
    }
}
