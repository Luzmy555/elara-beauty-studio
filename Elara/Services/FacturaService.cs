using ElaraMVC.Models;
using ElaraMVC.Repositories.Interfaces;
using ElaraMVC.Services.Interfaces;
using ElaraMVC.ViewModels;

namespace ElaraMVC.Services;

public class FacturaService : IFacturaService
{
    private readonly IFacturaRepository _repository;
    private readonly ICitaRepository _citaRepository;

    public FacturaService(IFacturaRepository repository, ICitaRepository citaRepository)
    {
        _repository = repository;
        _citaRepository = citaRepository;
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

        var factura = new Factura
        {
            CitaId = cita.Id,
            ClienteId = cita.ClienteId,
            EmpleadoId = cita.EmpleadoId,
            Subtotal = subtotal,
            Descuento = model.Descuento,
            DescuentoJustificacion = model.Descuento > 0 ? model.DescuentoJustificacion!.Trim() : null,
            Total = total,
            MetodoPago = model.MetodoPago,
            Estado = model.Estado,
            FechaEmision = DateTime.Now,
            ComisionEmpleado = comision
        };

        await _repository.AddAsync(factura);
        await _repository.SaveChangesAsync();

        return (true, null, factura.Id);
    }

    public async Task<ReporteCajaViewModel> ObtenerReporteCajaAsync(DateTime fecha)
    {
        var desde = fecha.Date;
        var hasta = desde.AddDays(1);

        var facturas = await _repository.GetEnRangoAsync(desde, hasta);
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
            Facturas = facturas
        };
    }

    public async Task<List<ComisionEmpleadoViewModel>> ObtenerReporteComisionesAsync(DateTime desde, DateTime hasta)
    {
        var facturas = await _repository.GetEnRangoAsync(desde.Date, hasta.Date.AddDays(1));

        return facturas
            .Where(f => f.Estado == EstadoFactura.Pagada)
            .GroupBy(f => new { f.EmpleadoId, Nombre = f.Empleado?.NombreCompleto ?? "—" })
            .Select(g => new ComisionEmpleadoViewModel
            {
                EmpleadoId = g.Key.EmpleadoId,
                NombreCompleto = g.Key.Nombre,
                CantidadServicios = g.Count(),
                TotalFacturado = g.Sum(f => f.Total),
                TotalComision = g.Sum(f => f.ComisionEmpleado)
            })
            .OrderByDescending(c => c.TotalComision)
            .ToList();
    }
}
