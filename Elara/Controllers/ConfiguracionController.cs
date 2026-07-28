using ElaraMVC.Services.Interfaces;
using ElaraMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElaraMVC.Controllers;

[Authorize(Roles = "Administrador")]
public class ConfiguracionController : Controller
{
    private readonly IConfiguracionNegocioService _configuracionService;
    private readonly ICategoriaServicioService _categoriaServicioService;

    public ConfiguracionController(
        IConfiguracionNegocioService configuracionService,
        ICategoriaServicioService categoriaServicioService)
    {
        _configuracionService = configuracionService;
        _categoriaServicioService = categoriaServicioService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = await _configuracionService.ConstruirFormularioAsync();
        ViewBag.Categorias = await _categoriaServicioService.ListarAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GuardarNegocio(ConfiguracionNegocioViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categorias = await _categoriaServicioService.ListarAsync();
            return View(nameof(Index), model);
        }

        var (success, error) = await _configuracionService.ActualizarAsync(model);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error!);
            ViewBag.Categorias = await _categoriaServicioService.ListarAsync();
            return View(nameof(Index), model);
        }

        TempData["SuccessMessage"] = "Datos del negocio actualizados correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult CategoriaCreate()
    {
        return View(new CategoriaServicioFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CategoriaCreate(CategoriaServicioFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (success, error) = await _categoriaServicioService.CrearAsync(model);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error!);
            return View(model);
        }

        TempData["SuccessMessage"] = $"Categoría \"{model.Nombre}\" creada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> CategoriaEdit(int id)
    {
        var categoria = await _categoriaServicioService.ObtenerPorIdAsync(id);
        if (categoria == null)
        {
            return NotFound();
        }

        var model = new CategoriaServicioFormViewModel
        {
            Id = categoria.Id,
            Nombre = categoria.Nombre,
            Activo = categoria.Activo
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CategoriaEdit(int id, CategoriaServicioFormViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (success, error) = await _categoriaServicioService.ActualizarAsync(model);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error!);
            return View(model);
        }

        TempData["SuccessMessage"] = $"Categoría \"{model.Nombre}\" actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CategoriaCambiarEstado(int id)
    {
        await _categoriaServicioService.CambiarEstadoAsync(id);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CategoriaEliminar(int id)
    {
        var (success, error) = await _categoriaServicioService.EliminarAsync(id);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "Categoría eliminada correctamente." : error;
        return RedirectToAction(nameof(Index));
    }
}
