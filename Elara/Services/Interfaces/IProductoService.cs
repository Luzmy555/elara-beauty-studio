using ElaraMVC.Models;
using ElaraMVC.ViewModels;

namespace ElaraMVC.Services.Interfaces;

public interface IProductoService
{
    Task<List<Producto>> ListarAsync(CategoriaProducto? categoria, string? term);
    Task<Producto?> ObtenerPorIdAsync(int id);
    Task<List<Producto>> ObtenerBajoStockAsync();
    Task<List<MovimientoInventario>> ObtenerMovimientosAsync(int productoId);
    Task<(bool Success, string? Error)> CrearAsync(ProductoFormViewModel model);
    Task<(bool Success, string? Error)> ActualizarAsync(ProductoFormViewModel model);
    Task<(bool Success, string? Error)> RegistrarMovimientoAsync(MovimientoFormViewModel model);
}
