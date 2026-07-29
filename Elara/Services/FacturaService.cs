using ElaraMVC.Models;
using ElaraMVC.Repositories.Interfaces;
using ElaraMVC.Services.Interfaces;
using ElaraMVC.ViewModels;
using Microsoft.AspNetCore.Http;

namespace ElaraMVC.Services;

public class FacturaService : IFacturaService
{
    private readonly IFacturaRepository _repository;
    private readonly ICitaRepository _citaRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IEmpleadoRepository _empleadoRepository;
    private readonly IServicioRepository _servicioRepository;
    private readonly IProductoRepository _productoRepository;
    private readonly IMovimientoInventarioRepository _movimientoInventarioRepository;
    private readonly IDevolucionRepository _devolucionRepository;
    private readonly IFotoUploadService _fotoUploadService;

    public FacturaService(
        IFacturaRepository repository,
        ICitaRepository citaRepository,
        IClienteRepository clienteRepository,
        IEmpleadoRepository empleadoRepository,
        IServicioRepository servicioRepository,
        IProductoRepository productoRepository,
        IMovimientoInventarioRepository movimientoInventarioRepository,
        IDevolucionRepository devolucionRepository,
        IFotoUploadService fotoUploadService)
    {
        _repository = repository;
        _citaRepository = citaRepository;
        _clienteRepository = clienteRepository;
        _empleadoRepository = empleadoRepository;
        _servicioRepository = servicioRepository;
        _productoRepository = productoRepository;
        _movimientoInventarioRepository = movimientoInventarioRepository;
        _devolucionRepository = devolucionRepository;
        _fotoUploadService = fotoUploadService;
    }

    public async Task<FacturaFormViewModel?> ConstruirFormularioAsync(int citaId)
    {
        var cita = await _citaRepository.GetByIdAsync(citaId);
        if (cita == null || cita.Estado != EstadoCita.Completada)
        {
            return null;
        }

        var facturaExistente = await _repository.GetByCitaIdAsync(citaId);
        if (facturaExistente != null)
        {
            return null;
        }

        return new FacturaFormViewModel
        {
            CitaId = cita.Id,
            ClienteNombre = cita.Cliente?.NombreCompleto ?? string.Empty,
            EmpleadoNombre = cita.Empleado?.NombreCompleto ?? string.Empty,
            ServicioNombre = cita.Servicio?.Nombre ?? string.Empty,
            Subtotal = cita.Servicio?.Precio ?? 0m
        };
    }

    public Task<Factura?> ObtenerPorIdAsync(int id) => _repository.GetByIdAsync(id);

    public async Task<(bool Success, string? Error, int? FacturaId)> CrearAsync(FacturaFormViewModel model)
    {
        var cita = await _citaRepository.GetByIdAsync(model.CitaId);
        if (cita == null)
        {
            return (false, "Cita no encontrada.", null);
        }

        if (cita.Estado != EstadoCita.Completada)
        {
            return (false, "Solo se puede facturar una cita marcada como Completada.", null);
        }

        var facturaExistente = await _repository.GetByCitaIdAsync(model.CitaId);
        if (facturaExistente != null)
        {
            return (false, "Esta cita ya tiene una factura generada.", facturaExistente.Id);
        }

        if (model.Descuento > 0 && string.IsNullOrWhiteSpace(model.DescuentoJustificacion))
        {
            return (false, "Indica la justificación del descuento.", null);
        }

        // El subtotal nunca se toma del formulario: siempre se recalcula desde
        // el servicio real de la cita, para que no se pueda manipular desde el cliente.
        var subtotal = cita.Servicio?.Precio ?? 0m;
        if (model.Descuento > subtotal)
        {
            return (false, "El descuento no puede ser mayor al subtotal.", null);
        }

        var total = subtotal - model.Descuento;
        var comision = Math.Round(total * (cita.Empleado?.ComisionPorcentaje ?? 0m) / 100m, 2);

        // La devuelta solo existe en efectivo; en tarjeta/transferencia se
        // ignora aunque el formulario traiga algo cargado.
        var montoRecibido = model.MetodoPago == MetodoPago.Efectivo ? model.MontoRecibido : null;
        if (montoRecibido.HasValue && montoRecibido.Value < total)
        {
            return (false, "El monto recibido es menor al total a cobrar.", null);
        }

        var factura = new Factura
        {
            CitaId = cita.Id,
            ClienteId = cita.ClienteId,
            Subtotal = subtotal,
            Descuento = model.Descuento,
            DescuentoJustificacion = model.Descuento > 0 ? model.DescuentoJustificacion!.Trim() : null,
            Total = total,
            MetodoPago = model.MetodoPago,
            Estado = model.Estado,
            MontoRecibido = montoRecibido,
            FechaEmision = DateTime.Now,
            FacturaDetalles = new List<FacturaDetalle>
            {
                new FacturaDetalle
                {
                    ServicioId = cita.ServicioId,
                    EmpleadoId = cita.EmpleadoId,
                    Cantidad = 1,
                    PrecioUnitario = subtotal,
                    Subtotal = subtotal,
                    ComisionEmpleado = comision
                }
            }
        };

        await GuardarConNumeroAsync(factura);

        return (true, null, factura.Id);
    }

    public async Task<VentaRapidaViewModel> ConstruirVentaRapidaFormularioAsync()
    {
        var clientes = await _clienteRepository.BuscarActivosAsync(null, 1000);
        var empleados = (await _empleadoRepository.GetAllAsync())
            .Where(e => e.Estado == EstadoEmpleado.Activo)
            .OrderBy(e => e.NombreCompleto)
            .ToList();
        var servicios = (await _servicioRepository.GetAllAsync())
            .Where(s => s.Activo)
            .OrderBy(s => s.Nombre)
            .ToList();
        var productos = (await _productoRepository.GetAllAsync(null, null))
            .Where(p => p.CantidadActual > 0)
            .OrderBy(p => p.Nombre)
            .ToList();

        return new VentaRapidaViewModel
        {
            ClientesDisponibles = clientes,
            EmpleadosDisponibles = empleados,
            ServiciosDisponibles = servicios,
            ProductosDisponibles = productos,
            Lineas = new List<VentaRapidaLineaViewModel> { new VentaRapidaLineaViewModel() }
        };
    }

    public async Task<(bool Success, string? Error, int? FacturaId)> CrearVentaRapidaAsync(VentaRapidaViewModel model)
    {
        // Una fila de servicio sin ServicioId es la fila vacía que trae el
        // formulario por defecto (venta de solo producto, nunca la tocaron):
        // se descarta en vez de tratarla como un error.
        var lineasServicio = (model.Lineas ?? new List<VentaRapidaLineaViewModel>())
            .Where(l => l.ServicioId.HasValue)
            .ToList();
        var hayServicios = lineasServicio.Count > 0;
        var hayProductos = model.LineasProductos != null && model.LineasProductos.Count > 0;
        if (!hayServicios && !hayProductos)
        {
            return (false, "Agrega al menos un servicio o producto a la venta.", null);
        }

        Cliente? cliente = null;
        if (model.ClienteId.HasValue)
        {
            cliente = await _clienteRepository.GetByIdAsync(model.ClienteId.Value);
            if (cliente == null)
            {
                return (false, "Cliente no encontrado.", null);
            }
        }

        var lineas = new List<(FacturaDetalle Detalle, decimal ComisionPorcentaje)>();

        if (hayServicios)
        {
            foreach (var linea in lineasServicio)
            {
                if (!linea.EmpleadoId.HasValue)
                {
                    return (false, "Selecciona quién atendió en cada línea de servicio.", null);
                }

                var servicio = await _servicioRepository.GetByIdAsync(linea.ServicioId!.Value);
                if (servicio == null)
                {
                    return (false, "Uno de los servicios seleccionados ya no existe.", null);
                }

                var empleado = await _empleadoRepository.GetByIdAsync(linea.EmpleadoId.Value);
                if (empleado == null)
                {
                    return (false, "Uno de los especialistas seleccionados ya no existe.", null);
                }

                var cantidad = linea.Cantidad < 1 ? 1 : linea.Cantidad;

                // A diferencia del flujo desde cita, aquí el precio SÍ lo decide
                // quien factura (venta rápida en caja: promociones, precio
                // negociado, etc.) — el catálogo solo sirve para precargar un
                // valor por defecto en el formulario.
                var precioUnitario = linea.PrecioUnitario;
                var subtotalLinea = precioUnitario * cantidad;

                lineas.Add((new FacturaDetalle
                {
                    ServicioId = servicio.Id,
                    EmpleadoId = empleado.Id,
                    Cantidad = cantidad,
                    PrecioUnitario = precioUnitario,
                    Subtotal = subtotalLinea
                }, empleado.ComisionPorcentaje));
            }
        }

        // Ventas de producto: valida stock disponible; la salida de inventario
        // real se registra después de conocer el Id de cada FacturaDetalle.
        var movimientosProducto = new List<(FacturaDetalle Detalle, Producto Producto, int Cantidad)>();
        if (hayProductos)
        {
            foreach (var lineaProd in model.LineasProductos!)
            {
                var producto = await _productoRepository.GetByIdAsync(lineaProd.ProductoId);
                if (producto == null)
                {
                    return (false, "Uno de los productos seleccionados ya no existe.", null);
                }

                var cantidad = lineaProd.Cantidad < 1 ? 1 : lineaProd.Cantidad;
                if (cantidad > producto.CantidadActual)
                {
                    return (false, $"No hay suficiente stock de \"{producto.Nombre}\": quedan {producto.CantidadActual:0.##} {producto.UnidadMedida}.", null);
                }

                Empleado? empleado = null;
                if (lineaProd.EmpleadoId.HasValue)
                {
                    empleado = await _empleadoRepository.GetByIdAsync(lineaProd.EmpleadoId.Value);
                    if (empleado == null)
                    {
                        return (false, "Uno de los especialistas seleccionados ya no existe.", null);
                    }
                }

                var precioUnitario = lineaProd.PrecioUnitario;
                var subtotalLinea = precioUnitario * cantidad;

                var detalle = new FacturaDetalle
                {
                    ProductoId = producto.Id,
                    EmpleadoId = empleado?.Id,
                    Cantidad = cantidad,
                    PrecioUnitario = precioUnitario,
                    Subtotal = subtotalLinea
                };

                lineas.Add((detalle, empleado?.ComisionPorcentaje ?? 0m));
                movimientosProducto.Add((detalle, producto, cantidad));
            }
        }

        if (model.Descuento > 0 && string.IsNullOrWhiteSpace(model.DescuentoJustificacion))
        {
            return (false, "Indica la justificación del descuento.", null);
        }

        var subtotalGeneral = lineas.Sum(l => l.Detalle.Subtotal);
        if (model.Descuento > subtotalGeneral)
        {
            return (false, "El descuento no puede ser mayor al subtotal.", null);
        }

        var total = subtotalGeneral - model.Descuento;

        // El descuento general se reparte entre las líneas según su peso en
        // el subtotal, y la comisión de cada especialista se calcula sobre su
        // parte ya con el descuento aplicado (igual que en el flujo desde cita).
        // Las líneas de producto sin especialista asignado quedan en comisión 0.
        foreach (var (detalle, comisionPorcentaje) in lineas)
        {
            var proporcion = subtotalGeneral == 0 ? 0m : detalle.Subtotal / subtotalGeneral;
            var descuentoLinea = Math.Round(model.Descuento * proporcion, 2);
            var totalLinea = detalle.Subtotal - descuentoLinea;
            detalle.ComisionEmpleado = comisionPorcentaje == 0
                ? 0m
                : Math.Round(totalLinea * comisionPorcentaje / 100m, 2);
        }

        var telefonoContacto = cliente == null && !string.IsNullOrWhiteSpace(model.ClienteTelefonoContacto)
            ? model.ClienteTelefonoContacto.Trim()
            : null;

        // La devuelta solo existe en efectivo; en tarjeta/transferencia se
        // ignora aunque el formulario traiga algo cargado.
        var montoRecibido = model.MetodoPago == MetodoPago.Efectivo ? model.MontoRecibido : null;
        if (montoRecibido.HasValue && montoRecibido.Value < total)
        {
            return (false, "El monto recibido es menor al total a cobrar.", null);
        }

        // El comprobante de transferencia se valida/guarda ANTES de tocar la
        // base de datos, igual que EmpleadoService.CrearAsync con la foto.
        string? comprobanteUrl = null;
        if (model.MetodoPago == MetodoPago.Transferencia && model.ComprobanteTransferencia != null)
        {
            var (success, pathOrError) = await _fotoUploadService.GuardarAsync(model.ComprobanteTransferencia, "comprobantes");
            if (!success)
            {
                return (false, pathOrError, null);
            }

            comprobanteUrl = pathOrError;
        }

        var factura = new Factura
        {
            ClienteId = cliente?.Id,
            ClienteTelefonoContacto = telefonoContacto,
            Subtotal = subtotalGeneral,
            Descuento = model.Descuento,
            DescuentoJustificacion = model.Descuento > 0 ? model.DescuentoJustificacion!.Trim() : null,
            Total = total,
            MetodoPago = model.MetodoPago,
            Estado = model.Estado,
            MontoRecibido = montoRecibido,
            ComprobanteTransferenciaUrl = comprobanteUrl,
            FechaEmision = DateTime.Now,
            FacturaDetalles = lineas.Select(l => l.Detalle).ToList()
        };

        await GuardarConNumeroAsync(factura);

        // Recién ahora cada FacturaDetalle tiene Id: se puede registrar la
        // salida de inventario enlazada a la línea que la originó.
        if (movimientosProducto.Count > 0)
        {
            foreach (var (detalle, producto, cantidad) in movimientosProducto)
            {
                producto.CantidadActual -= cantidad;
                _productoRepository.Update(producto);

                await _movimientoInventarioRepository.AddAsync(new MovimientoInventario
                {
                    ProductoId = producto.Id,
                    Tipo = TipoMovimiento.Salida,
                    Cantidad = cantidad,
                    Fecha = DateTime.Now,
                    Motivo = $"Venta - Factura {factura.NumeroFactura}",
                    FacturaDetalleId = detalle.Id
                });
            }

            await _movimientoInventarioRepository.SaveChangesAsync();
        }

        return (true, null, factura.Id);
    }

    public async Task<(bool Success, string? Error)> SubirComprobanteTransferenciaAsync(int facturaId, IFormFile archivo)
    {
        var factura = await _repository.GetByIdAsync(facturaId);
        if (factura == null)
        {
            return (false, "Factura no encontrada.");
        }

        var (success, pathOrError) = await _fotoUploadService.GuardarAsync(archivo, "comprobantes");
        if (!success)
        {
            return (false, pathOrError);
        }

        _fotoUploadService.Eliminar(factura.ComprobanteTransferenciaUrl);
        factura.ComprobanteTransferenciaUrl = pathOrError;
        await _repository.SaveChangesAsync();

        return (true, null);
    }

    // El correlativo (ej. "ELR-0001") se deriva del Id autoincremental, así
    // que solo se puede fijar en un segundo guardado, una vez que el insert
    // ya asignó el Id.
    private async Task GuardarConNumeroAsync(Factura factura)
    {
        await _repository.AddAsync(factura);
        await _repository.SaveChangesAsync();

        factura.NumeroFactura = $"ELR-{factura.Id:D4}";
        await _repository.SaveChangesAsync();
    }

    public async Task<ReporteCajaViewModel> ObtenerReporteCajaAsync(DateTime fecha)
    {
        var desde = fecha.Date;
        var hasta = desde.AddDays(1);

        var facturas = await _repository.GetEnRangoAsync(desde, hasta);
        var devoluciones = await _devolucionRepository.GetEnRangoAsync(desde, hasta);
        var pagadas = facturas.Where(f => f.Estado == EstadoFactura.Pagada).ToList();
        var pendientes = facturas.Where(f => f.Estado == EstadoFactura.Pendiente).ToList();

        return new ReporteCajaViewModel
        {
            Fecha = desde,
            TotalEfectivo = pagadas.Where(f => f.MetodoPago == MetodoPago.Efectivo).Sum(f => f.Total),
            TotalTarjeta = pagadas.Where(f => f.MetodoPago == MetodoPago.Tarjeta).Sum(f => f.Total),
            TotalTransferencia = pagadas.Where(f => f.MetodoPago == MetodoPago.Transferencia).Sum(f => f.Total),
            TotalPendiente = pendientes.Sum(f => f.Total),
            CantidadFacturas = pagadas.Count,
            CantidadDesdeCita = facturas.Count(f => f.CitaId != null),
            CantidadVentaRapida = facturas.Count(f => f.CitaId == null),
            Facturas = facturas,
            TotalDevoluciones = devoluciones.Sum(d => d.MontoReembolsado),
            Devoluciones = devoluciones
        };
    }

    public async Task<List<ComisionEmpleadoViewModel>> ObtenerReporteComisionesAsync(DateTime desde, DateTime hasta)
    {
        var facturas = await _repository.GetEnRangoAsync(desde.Date, hasta.Date.AddDays(1));

        var lineas = facturas
            .Where(f => f.Estado == EstadoFactura.Pagada)
            .SelectMany(f => f.FacturaDetalles
                .Where(d => d.EmpleadoId.HasValue)
                .Select(d => new
                {
                    EmpleadoId = d.EmpleadoId!.Value,
                    Nombre = d.Empleado?.NombreCompleto ?? "—",
                    // Recalcula el total de la línea con el descuento general de
                    // su factura ya prorrateado, igual que al momento de emitirla.
                    TotalLinea = d.Subtotal - (f.Subtotal == 0 ? 0m : Math.Round(f.Descuento * d.Subtotal / f.Subtotal, 2)),
                    d.ComisionEmpleado
                }));

        return lineas
            .GroupBy(x => new { x.EmpleadoId, x.Nombre })
            .Select(g => new ComisionEmpleadoViewModel
            {
                EmpleadoId = g.Key.EmpleadoId,
                NombreCompleto = g.Key.Nombre,
                CantidadServicios = g.Count(),
                TotalFacturado = g.Sum(x => x.TotalLinea),
                TotalComision = g.Sum(x => x.ComisionEmpleado)
            })
            .OrderByDescending(c => c.TotalComision)
            .ToList();
    }
}
