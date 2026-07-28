using ElaraMVC.Services.Interfaces;
using ElaraMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElaraMVC.Controllers;

[Authorize(Roles = "Administrador,Recepcionista")]
public class ClientesController : Controller
{
    private readonly IClienteService _clienteService;

    public ClientesController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? term, int page = 1)
    {
        var model = await _clienteService.BuscarAsync(term, page);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Buscar(string? term, int page = 1)
    {
        var model = await _clienteService.BuscarAsync(term, page);
        return PartialView("_ClienteCards", model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var cliente = await _clienteService.ObtenerPorIdAsync(id);
        if (cliente == null)
        {
            return NotFound();
        }

        // Espacio preparado para el módulo de Citas: cuando exista Cita con FK
        // a ClienteId, inyectar aquí ICitaService y cargar el historial de citas
        // pasadas de este cliente para pasarlo a la vista.

        return View(cliente);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new ClienteFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClienteFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (success, error) = await _clienteService.CrearAsync(model);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error!);
            return View(model);
        }

        TempData["SuccessMessage"] = $"Cliente \"{model.NombreCompleto}\" registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var cliente = await _clienteService.ObtenerPorIdAsync(id);
        if (cliente == null)
        {
            return NotFound();
        }

        var model = new ClienteFormViewModel
        {
            Id = cliente.Id,
            NombreCompleto = cliente.NombreCompleto,
            Telefono = cliente.Telefono,
            Email = cliente.Email,
            FechaNacimiento = cliente.FechaNacimiento,
            Alergias = cliente.Alergias,
            Preferencias = cliente.Preferencias,
            FotoActualUrl = cliente.FotoUrl,
            Activo = cliente.Activo
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ClienteFormViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (success, error) = await _clienteService.ActualizarAsync(model);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error!);
            return View(model);
        }

        TempData["SuccessMessage"] = $"Cliente \"{model.NombreCompleto}\" actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id, string? term, int page = 1)
    {
        await _clienteService.CambiarEstadoAsync(id);
        return RedirectToAction(nameof(Index), new { term, page });
    }
}
