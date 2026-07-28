using ElaraMVC.Models;
using ElaraMVC.Services.Interfaces;
using ElaraMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElaraMVC.Controllers;

[Authorize(Roles = "Administrador")]
public class ProductosController : Controller
{
    private readonly IProductoService _productoService;

    public ProductosController(IProductoService productoService)
    {
        _productoService = productoService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CategoriaProducto? categoria, string? term)
    {
        var productos = await _productoService.ListarAsync(categoria, term);
        var bajoStock = await _productoService.ObtenerBajoStockAsync();

        var model = new ProductoIndexViewModel
        {
            Productos = productos,
            CategoriaSeleccionada = categoria?.ToString(),
            Term = term,
            TotalBajoStock = bajoStock.Count
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Reabastecer()
    {
        var productos = await _productoService.ObtenerBajoStockAsync();
        return View(productos);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var producto = await _productoService.ObtenerPorIdAsync(id);
        if (producto == null)
        {
            return NotFound();
        }

        var model = new ProductoDetalleViewModel
        {
            Producto = producto,
            Movimientos = await _productoService.ObtenerMovimientosAsync(id)
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new ProductoFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductoFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (success, error) = await _productoService.CrearAsync(model);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error!);
            return View(model);
        }

        TempData["SuccessMessage"] = $"Producto \"{model.Nombre}\" registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var producto = await _productoService.ObtenerPorIdAsync(id);
        if (producto == null)
        {
            return NotFound();
        }

        var model = new ProductoFormViewModel
        {
            Id = producto.Id,
            Nombre = producto.Nombre,
            Categoria = producto.Categoria,
            Marca = producto.Marca,
            CantidadActual = producto.CantidadActual,
            CantidadMinima = producto.CantidadMinima,
            UnidadMedida = producto.UnidadMedida,
            PrecioCosto = producto.PrecioCosto,
            PrecioVenta = producto.PrecioVenta,
            Proveedor = producto.Proveedor,
            FechaUltimaCompra = producto.FechaUltimaCompra,
            EsEdicion = true
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductoFormViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (success, error) = await _productoService.ActualizarAsync(model);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error!);
            return View(model);
        }

        TempData["SuccessMessage"] = $"Producto \"{model.Nombre}\" actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarMovimiento(MovimientoFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Revisa los datos del movimiento: " +
                string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return RedirectToAction(nameof(Details), new { id = model.ProductoId });
        }

        var (success, error) = await _productoService.RegistrarMovimientoAsync(model);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success
            ? "Movimiento registrado correctamente."
            : error;

        return RedirectToAction(nameof(Details), new { id = model.ProductoId });
    }
}
