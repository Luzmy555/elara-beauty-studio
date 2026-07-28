using ElaraMVC.Services.Interfaces;
using ElaraMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElaraMVC.Controllers;

// El listado lo puede ver Administrador y Recepcionista (para agendar citas);
// crear, editar y desactivar servicios queda restringido solo a Administrador.
[Authorize(Roles = "Administrador,Recepcionista")]
public class ServiciosController : Controller
{
    private readonly IServicioService _servicioService;
    private readonly ICategoriaServicioService _categoriaServicioService;

    public ServiciosController(IServicioService servicioService, ICategoriaServicioService categoriaServicioService)
    {
        _servicioService = servicioService;
        _categoriaServicioService = categoriaServicioService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var servicios = await _servicioService.ListarAsync();
        ViewBag.Categorias = await _categoriaServicioService.ListarActivasAsync();
        return View(servicios);
    }

    [Authorize(Roles = "Administrador")]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new ServicioFormViewModel();
        await CargarCategoriasAsync(model);
        return View(model);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServicioFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await CargarCategoriasAsync(model);
            return View(model);
        }

        var (success, error) = await _servicioService.CrearAsync(model);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error!);
            await CargarCategoriasAsync(model);
            return View(model);
        }

        TempData["SuccessMessage"] = $"Servicio \"{model.Nombre}\" creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrador")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var servicio = await _servicioService.ObtenerPorIdAsync(id);
        if (servicio == null)
        {
            return NotFound();
        }

        var model = new ServicioFormViewModel
        {
            Id = servicio.Id,
            Nombre = servicio.Nombre,
            CategoriaServicioId = servicio.CategoriaServicioId,
            Descripcion = servicio.Descripcion,
            DuracionMinutos = servicio.DuracionMinutos,
            Precio = servicio.Precio,
            ImagenActualUrl = servicio.ImagenUrl,
            Activo = servicio.Activo
        };

        await CargarCategoriasAsync(model);
        return View(model);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ServicioFormViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await CargarCategoriasAsync(model);
            return View(model);
        }

        var (success, error) = await _servicioService.ActualizarAsync(model);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error!);
            await CargarCategoriasAsync(model);
            return View(model);
        }

        TempData["SuccessMessage"] = $"Servicio \"{model.Nombre}\" actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id)
    {
        await _servicioService.CambiarEstadoAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private async Task CargarCategoriasAsync(ServicioFormViewModel model)
    {
        model.CategoriasDisponibles = await _categoriaServicioService.ListarActivasAsync();
    }
}
