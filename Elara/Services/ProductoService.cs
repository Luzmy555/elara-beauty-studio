using ElaraMVC.Models;
using ElaraMVC.Repositories.Interfaces;
using ElaraMVC.Services.Interfaces;
using ElaraMVC.ViewModels;

namespace ElaraMVC.Services;

public class ProductoService : IProductoService
{
    private readonly IProductoRepository _repository;
    private readonly IMovimientoInventarioRepository _movimientoRepository;

    public ProductoService(IProductoRepository repository, IMovimientoInventarioRepository movimientoRepository)
    {
        _repository = repository;
        _movimientoRepository = movimientoRepository;
    }

    public Task<List<Producto>> ListarAsync(CategoriaProducto? categoria, string? term) =>
        _repository.GetAllAsync(categoria, term);

    public Task<Producto?> ObtenerPorIdAsync(int id) => _repository.GetByIdAsync(id);

    public Task<List<Producto>> ObtenerBajoStockAsync() => _repository.GetBajoStockAsync();

    public Task<List<MovimientoInventario>> ObtenerMovimientosAsync(int productoId) =>
        _movimientoRepository.GetPorProductoAsync(productoId);

    public async Task<(bool Success, string? Error)> CrearAsync(ProductoFormViewModel model)
    {
        var producto = new Producto
        {
            Nombre = model.Nombre.Trim(),
            Categoria = model.Categoria,
            Marca = model.Marca.Trim(),
            CantidadActual = model.CantidadActual,
            CantidadMinima = model.CantidadMinima,
            UnidadMedida = model.UnidadMedida,
            PrecioCosto = model.PrecioCosto,
            Proveedor = model.Proveedor?.Trim(),
            FechaUltimaCompra = model.CantidadActual > 0 ? DateTime.Now : null
        };

        await _repository.AddAsync(producto);
        await _repository.SaveChangesAsync();

        if (model.CantidadActual > 0)
        {
            await _movimientoRepository.AddAsync(new MovimientoInventario
            {
                ProductoId = producto.Id,
                Tipo = TipoMovimiento.Entrada,
                Cantidad = model.CantidadActual,
                Fecha = DateTime.Now,
                Motivo = "Stock inicial al registrar el producto"
            });
            await _movimientoRepository.SaveChangesAsync();
        }

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ActualizarAsync(ProductoFormViewModel model)
    {
        var producto = await _repository.GetByIdAsync(model.Id);
        if (producto == null)
        {
            return (false, "Producto no encontrado.");
        }

        producto.Nombre = model.Nombre.Trim();
        producto.Categoria = model.Categoria;
        producto.Marca = model.Marca.Trim();
        producto.CantidadMinima = model.CantidadMinima;
        producto.UnidadMedida = model.UnidadMedida;
        producto.PrecioCosto = model.PrecioCosto;
        producto.Proveedor = model.Proveedor?.Trim();
        // CantidadActual y FechaUltimaCompra NO se editan aquí: solo cambian a
        // través de RegistrarMovimientoAsync, para conservar el historial real.

        _repository.Update(producto);
        await _repository.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> RegistrarMovimientoAsync(MovimientoFormViewModel model)
    {
        var producto = await _repository.GetByIdAsync(model.ProductoId);
        if (producto == null)
        {
            return (false, "Producto no encontrado.");
        }

        if (model.Tipo == TipoMovimiento.Salida && model.Cantidad > producto.CantidadActual)
        {
            return (false, $"No hay suficiente stock: quedan {producto.CantidadActual:0.##} {producto.UnidadMedida}.");
        }

        producto.CantidadActual += model.Tipo == TipoMovimiento.Entrada ? model.Cantidad : -model.Cantidad;

        if (model.Tipo == TipoMovimiento.Entrada)
        {
            producto.FechaUltimaCompra = DateTime.Now;
        }

        _repository.Update(producto);

        await _movimientoRepository.AddAsync(new MovimientoInventario
        {
            ProductoId = producto.Id,
            Tipo = model.Tipo,
            Cantidad = model.Cantidad,
            Fecha = DateTime.Now,
            Motivo = model.Motivo.Trim()
        });

        // Ambos cambios (Producto y MovimientoInventario) comparten el mismo
        // DbContext, así que un solo SaveChanges los confirma juntos.
        await _repository.SaveChangesAsync();
        return (true, null);
    }
}
