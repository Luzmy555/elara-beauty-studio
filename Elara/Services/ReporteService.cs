using System.Globalization;
using ElaraMVC.Data;
using ElaraMVC.Models;
using ElaraMVC.Services.Interfaces;
using ElaraMVC.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ElaraMVC.Services;

// Servicio de solo lectura para el dashboard de Reportes: cruza Factura,
// Cita, Cliente y Producto con agregaciones LINQ. Igual que
// DisponibilidadService, consulta el DbContext directamente en vez de
// repositorios por entidad, porque son consultas analíticas que combinan
// varios agregados a la vez.
public class ReporteService : IReporteService
{
    private static readonly CultureInfo CulturaEs = CultureInfo.GetCultureInfo("es-ES");

    private readonly ApplicationDbContext _context;

    public ReporteService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ReporteKpiViewModel> ObtenerKpisAsync()
    {
        var hoy = DateTime.Today;
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
        var finMes = inicioMes.AddMonths(1);
        var finHoy = hoy.AddDays(1);

        var ingresosMes = await _context.Facturas
            .Where(f => f.Estado == EstadoFactura.Pagada && f.FechaEmision >= inicioMes && f.FechaEmision < finMes)
            .SumAsync(f => (decimal?)f.Total) ?? 0m;

        var citasHoy = await _context.Citas
            .CountAsync(c => c.FechaHoraInicio >= hoy && c.FechaHoraInicio < finHoy && c.Estado != EstadoCita.Cancelada);

        var clientesActivos = await _context.Clientes.CountAsync(c => c.Activo);

        var productosBajoStock = await _context.Productos.CountAsync(p => p.CantidadActual < p.CantidadMinima);

        return new ReporteKpiViewModel
        {
            IngresosDelMes = ingresosMes,
            CitasDeHoy = citasHoy,
            ClientesActivos = clientesActivos,
            ProductosPorReabastecer = productosBajoStock
        };
    }

    public async Task<List<SerieMensualViewModel>> ObtenerIngresosPorMesAsync(DateTime desde, DateTime hasta)
    {
        var facturas = await _context.Facturas
            .Where(f => f.Estado == EstadoFactura.Pagada && f.FechaEmision >= desde && f.FechaEmision < hasta)
            .Select(f => new { f.FechaEmision, f.Total })
            .ToListAsync();

        var agrupado = facturas
            .GroupBy(f => new DateTime(f.FechaEmision.Year, f.FechaEmision.Month, 1))
            .Select(g => new SerieMensualViewModel
            {
                Periodo = g.Key,
                Etiqueta = FormatearMes(g.Key),
                Valor = g.Sum(f => f.Total)
            })
            .ToList();

        return CompletarMesesFaltantes(agrupado, desde, hasta);
    }

    public async Task<List<CategoriaConteoViewModel>> ObtenerServiciosMasSolicitadosAsync(DateTime desde, DateTime hasta)
    {
        // "Solicitados" = citas agendadas (cualquier estado salvo Cancelada),
        // no solo las ya completadas: refleja la demanda real del servicio.
        return await _context.Citas
            .Where(c => c.Estado != EstadoCita.Cancelada && c.FechaHoraInicio >= desde && c.FechaHoraInicio < hasta)
            .GroupBy(c => c.Servicio!.Nombre)
            .Select(g => new CategoriaConteoViewModel
            {
                Nombre = g.Key,
                Cantidad = g.Count()
            })
            .OrderByDescending(s => s.Cantidad)
            .Take(8)
            .ToListAsync();
    }

    public async Task<List<RankingEmpleadoViewModel>> ObtenerRankingEmpleadosAsync(DateTime desde, DateTime hasta)
    {
        return await _context.Citas
            .Where(c => c.Estado == EstadoCita.Completada && c.FechaHoraInicio >= desde && c.FechaHoraInicio < hasta)
            .GroupBy(c => c.Empleado!.NombreCompleto)
            .Select(g => new RankingEmpleadoViewModel
            {
                NombreCompleto = g.Key,
                CitasCompletadas = g.Count()
            })
            .OrderByDescending(r => r.CitasCompletadas)
            .Take(10)
            .ToListAsync();
    }

    public async Task<List<SerieMensualViewModel>> ObtenerClientesNuevosPorMesAsync(DateTime desde, DateTime hasta)
    {
        var fechas = await _context.Clientes
            .Where(c => c.FechaRegistro >= desde && c.FechaRegistro < hasta)
            .Select(c => c.FechaRegistro)
            .ToListAsync();

        var agrupado = fechas
            .GroupBy(f => new DateTime(f.Year, f.Month, 1))
            .Select(g => new SerieMensualViewModel
            {
                Periodo = g.Key,
                Etiqueta = FormatearMes(g.Key),
                Valor = g.Count()
            })
            .ToList();

        return CompletarMesesFaltantes(agrupado, desde, hasta);
    }

    public async Task<DashboardViewModel> ObtenerDashboardAsync()
    {
        var ahora = DateTime.Now;
        var hoy = DateTime.Today;
        var finHoy = hoy.AddDays(1);
        var inicioSemana = InicioSemana(hoy);

        var citasHoy = await _context.Citas
            .CountAsync(c => c.FechaHoraInicio >= hoy && c.FechaHoraInicio < finHoy && c.Estado != EstadoCita.Cancelada);

        var ingresosHoy = await _context.Facturas
            .Where(f => f.Estado == EstadoFactura.Pagada && f.FechaEmision >= hoy && f.FechaEmision < finHoy)
            .SumAsync(f => (decimal?)f.Total) ?? 0m;

        var clientesNuevosSemana = await _context.Clientes
            .CountAsync(c => c.FechaRegistro >= inicioSemana && c.FechaRegistro < finHoy);

        var productosBajoStock = await _context.Productos.CountAsync(p => p.CantidadActual < p.CantidadMinima);

        // Próximas citas de hoy: desde la hora actual en adelante, para que la lista
        // del dashboard no muestre citas que ya pasaron durante el día.
        var proximasCitas = await _context.Citas
            .Where(c => c.FechaHoraInicio >= ahora && c.FechaHoraInicio < finHoy && c.Estado != EstadoCita.Cancelada)
            .OrderBy(c => c.FechaHoraInicio)
            .Take(8)
            .Select(c => new ProximaCitaViewModel
            {
                FechaHoraInicio = c.FechaHoraInicio,
                Cliente = c.Cliente!.NombreCompleto,
                Empleado = c.Empleado!.NombreCompleto,
                Servicio = c.Servicio!.Nombre,
                Estado = c.Estado.ToString()
            })
            .ToListAsync();

        return new DashboardViewModel
        {
            CitasHoy = citasHoy,
            IngresosHoy = ingresosHoy,
            ClientesNuevosSemana = clientesNuevosSemana,
            ProductosPorReabastecer = productosBajoStock,
            ProximasCitasHoy = proximasCitas
        };
    }

    // Lunes como primer día de la semana (convención local), no domingo.
    private static DateTime InicioSemana(DateTime fecha)
    {
        var diferencia = ((int)fecha.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return fecha.AddDays(-diferencia);
    }

    private static string FormatearMes(DateTime fecha) => fecha.ToString("MMM yyyy", CulturaEs);

    // Rellena con 0 los meses del rango que no tuvieron datos, para que el
    // gráfico muestre siempre el eje completo en vez de solo los meses con actividad.
    private static List<SerieMensualViewModel> CompletarMesesFaltantes(List<SerieMensualViewModel> datos, DateTime desde, DateTime hasta)
    {
        var resultado = new List<SerieMensualViewModel>();
        var cursor = new DateTime(desde.Year, desde.Month, 1);

        // "hasta" es exclusivo (medianoche del día siguiente al último día
        // incluido); el último mes a mostrar es el que contiene "hasta - 1 día".
        var ultimoDiaIncluido = hasta.AddDays(-1);
        var limite = new DateTime(ultimoDiaIncluido.Year, ultimoDiaIncluido.Month, 1);

        while (cursor <= limite)
        {
            var existente = datos.FirstOrDefault(d => d.Periodo == cursor);
            resultado.Add(existente ?? new SerieMensualViewModel
            {
                Periodo = cursor,
                Etiqueta = FormatearMes(cursor),
                Valor = 0
            });
            cursor = cursor.AddMonths(1);
        }

        return resultado;
    }
}
