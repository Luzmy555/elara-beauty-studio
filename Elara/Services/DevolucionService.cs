using ElaraMVC.Models;
using ElaraMVC.Repositories.Interfaces;
using ElaraMVC.Services.Interfaces;
using ElaraMVC.ViewModels;

namespace ElaraMVC.Services;

public class DevolucionService : IDevolucionService
{
    private readonly IDevolucionRepository _repository;
    private readonly IProductoRepository _productoRepository;
    private readonly IMovimientoInventarioRepository _movimientoInventarioRepository;

    public DevolucionService(
        IDevolucionRepository repository,
        IProductoRepository productoRepository,
        IMovimientoInventarioRepository movimientoInventarioRepository)
    {
        _repository = repository;
        _productoRepository = productoRepository;
        _movimientoInventarioRepository = movimientoInventarioRepository;
    }

    public async Task<DevolucionFormViewModel?> ConstruirFormularioAsync(int facturaDetalleId)
    {
        var detalle = await _repository.GetFacturaDetalleConDevolucionesAsync(facturaDetalleId);
        if (detalle == null || detalle.ProductoId == null)
        {
            return null;
        }

        var yaDevuelto = detalle.Devoluciones.Sum(d => d.Cantidad);
        var disponible = detalle.Cantidad - yaDevuelto;

        return new DevolucionFormViewModel
        {
            FacturaDetalleId = detalle.Id,
            ProductoNombre = detalle.Producto?.Nombre ?? string.Empty,
            CantidadFacturada = detalle.Cantidad,
            CantidadYaDevuelta = yaDevuelto,
            CantidadDisponible = disponible,
            PrecioUnitario = detalle.PrecioUnitario,
            Cantidad = disponible > 0 ? 1 : 0
        };
    }

    public async Task<(bool Success, string? Error, int? DevolucionId)> CrearAsync(DevolucionFormViewModel model, string usuarioId)
    {
        var detalle = await _repository.GetFacturaDetalleConDevolucionesAsync(model.FacturaDetalleId);
        if (detalle == null || detalle.ProductoId == null || detalle.Producto == null)
        {
            return (false, "La línea de producto no existe.", null);
        }

        if (model.Cantidad < 1)
        {
            return (false, "La cantidad a devolver debe ser al menos 1.", null);
        }

        var yaDevuelto = detalle.Devoluciones.Sum(d => d.Cantidad);
        var disponible = detalle.Cantidad - yaDevuelto;
        if (model.Cantidad > disponible)
        {
            return (false, $"Solo quedan {disponible} unidad(es) disponibles para devolver de esta línea.", null);
        }

        if (string.IsNullOrWhiteSpace(model.Motivo))
        {
            return (false, "Indica el motivo de la devolución.", null);
        }

        // El monto se calcula desde el precio real registrado en la línea,
        // nunca de un valor que venga del formulario.
        var precioPromedioLinea = detalle.Cantidad == 0 ? 0m : detalle.Subtotal / detalle.Cantidad;
        var montoReembolsado = Math.Round(precioPromedioLinea * model.Cantidad, 2);

        var devolucion = new Devolucion
        {
            FacturaDetalleId = detalle.Id,
            Cantidad = model.Cantidad,
            MontoReembolsado = montoReembolsado,
            Motivo = model.Motivo.Trim(),
            MetodoReembolso = model.MetodoReembolso,
            Fecha = DateTime.Now,
            ProcesadoPorUserId = usuarioId
        };

        await _repository.AddAsync(devolucion);
        await _repository.SaveChangesAsync();

        devolucion.NumeroDevolucion = $"DEV-{devolucion.Id:D4}";
        await _repository.SaveChangesAsync();

        // Reintegra el producto al stock, con su propio movimiento de entrada
        // enlazado a la línea original para trazabilidad.
        var producto = detalle.Producto;
        producto.CantidadActual += model.Cantidad;
        _productoRepository.Update(producto);

        await _movimientoInventarioRepository.AddAsync(new MovimientoInventario
        {
            ProductoId = producto.Id,
            Tipo = TipoMovimiento.Entrada,
            Cantidad = model.Cantidad,
            Fecha = DateTime.Now,
            Motivo = $"Devolución {devolucion.NumeroDevolucion}",
            FacturaDetalleId = detalle.Id
        });
        await _movimientoInventarioRepository.SaveChangesAsync();

        return (true, null, devolucion.Id);
    }

    public Task<Devolucion?> ObtenerPorIdAsync(int id) => _repository.GetByIdAsync(id);
}
