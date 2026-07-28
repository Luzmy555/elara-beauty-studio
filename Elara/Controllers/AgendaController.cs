using ElaraMVC.Models;
using ElaraMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ElaraMVC.Controllers;

// El Especialista solo ve y gestiona sus propias citas: no hay acciones de
// crear ni reagendar aquí (eso es exclusivo de Administrador/Recepcionista
// en CitasController). CambiarEstado valida en el servicio que la cita
// pertenezca al empleado autenticado antes de tocarla.
[Authorize(Roles = "Especialista")]
public class AgendaController : Controller
{
    private readonly IEmpleadoService _empleadoService;
    private readonly ICitaService _citaService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AgendaController(IEmpleadoService empleadoService, ICitaService citaService, UserManager<ApplicationUser> userManager)
    {
        _empleadoService = empleadoService;
        _citaService = citaService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var empleado = await ObtenerEmpleadoActualAsync();
        return View(empleado);
    }

    [HttpGet]
    public async Task<IActionResult> Eventos(DateTime start, DateTime end)
    {
        var empleado = await ObtenerEmpleadoActualAsync();
        if (empleado == null)
        {
            return Json(Array.Empty<object>());
        }

        var eventos = await _citaService.ObtenerEventosAsync(start, end, empleado.Id);
        return Json(eventos);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id, EstadoCita estado)
    {
        var empleado = await ObtenerEmpleadoActualAsync();
        if (empleado == null)
        {
            return Json(new { success = false, error = "No se encontró tu perfil de empleada." });
        }

        var (success, error) = await _citaService.CambiarEstadoAsync(id, estado, empleado.Id);
        return Json(new { success, error });
    }

    private async Task<Empleado?> ObtenerEmpleadoActualAsync()
    {
        var userId = _userManager.GetUserId(User);
        return userId == null ? null : await _empleadoService.ObtenerPorUsuarioAsync(userId);
    }
}
