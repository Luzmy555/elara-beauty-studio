using ElaraMVC.Models;
using ElaraMVC.Repositories.Interfaces;
using ElaraMVC.Services.Interfaces;
using ElaraMVC.ViewModels;

namespace ElaraMVC.Services;

public class CitaService : ICitaService
{
    private readonly ICitaRepository _repository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IServicioRepository _servicioRepository;
    private readonly IEmpleadoRepository _empleadoRepository;
    private readonly IDisponibilidadService _disponibilidadService;
    private readonly IFacturaRepository _facturaRepository;

    public CitaService(
        ICitaRepository repository,
        IClienteRepository clienteRepository,
        IServicioRepository servicioRepository,
        IEmpleadoRepository empleadoRepository,
        IDisponibilidadService disponibilidadService,
        IFacturaRepository facturaRepository)
    {
        _repository = repository;
        _clienteRepository = clienteRepository;
        _servicioRepository = servicioRepository;
        _empleadoRepository = empleadoRepository;
        _disponibilidadService = disponibilidadService;
        _facturaRepository = facturaRepository;
    }

    public async Task<List<CitaEventoViewModel>> ObtenerEventosAsync(DateTime desde, DateTime hasta, int? empleadoId = null)
    {
        var citas = await _repository.GetEnRangoAsync(desde, hasta, empleadoId);
        var facturaIdsPorCita = await _facturaRepository.GetFacturaIdsPorCitaAsync(citas.Select(c => c.Id));

        return citas.Select(c =>
        {
            facturaIdsPorCita.TryGetValue(c.Id, out var facturaId);

            return new CitaEventoViewModel
            {
                Id = c.Id,
                ClienteNombre = c.Cliente?.NombreCompleto ?? string.Empty,
                ServicioId = c.ServicioId,
                ServicioNombre = c.Servicio?.Nombre ?? string.Empty,
                EmpleadoId = c.EmpleadoId,
                EmpleadoNombre = c.Empleado?.NombreCompleto ?? string.Empty,
                Start = c.FechaHoraInicio,
                End = c.FechaHoraFin,
                Estado = c.Estado.ToString(),
                PuedeEditar = c.Estado is EstadoCita.Pendiente or EstadoCita.Confirmada,
                FacturaId = facturaId == 0 ? null : facturaId
            };
        }).ToList();
    }

    public Task<Cita?> ObtenerPorIdAsync(int id) => _repository.GetByIdAsync(id);

    public async Task<CitaFormViewModel> ConstruirFormularioNuevoAsync(DateTime? fechaSugerida)
    {
        return new CitaFormViewModel
        {
            FechaHoraInicio = fechaSugerida ?? RedondearProximaHora(DateTime.Now),
            ServiciosDisponibles = await _servicioRepository.GetAllAsync(),
            EmpleadosDisponibles = await ObtenerEmpleadosActivosAsync()
        };
    }

    public async Task<CitaFormViewModel?> ConstruirFormularioEdicionAsync(int id)
    {
        var cita = await _repository.GetByIdAsync(id);
        if (cita == null)
        {
            return null;
        }

        return new CitaFormViewModel
        {
            Id = cita.Id,
            ClienteId = cita.ClienteId,
            ClienteNombre = cita.Cliente?.NombreCompleto,
            ServicioId = cita.ServicioId,
            EmpleadoId = cita.EmpleadoId,
            FechaHoraInicio = cita.FechaHoraInicio,
            Notas = cita.Notas,
            Estado = cita.Estado,
            EsEdicion = true,
            ServiciosDisponibles = await _servicioRepository.GetAllAsync(),
            EmpleadosDisponibles = await ObtenerEmpleadosActivosAsync()
        };
    }

    private async Task<List<Empleado>> ObtenerEmpleadosActivosAsync()
    {
        var empleados = await _empleadoRepository.GetAllAsync();
        return empleados
            .Where(e => e.Estado == EstadoEmpleado.Activo)
            .OrderBy(e => e.NombreCompleto)
            .ToList();
    }

    public async Task<(bool Success, string? Error, int? CitaId)> CrearAsync(CitaFormViewModel model)
    {
        var servicio = await _servicioRepository.GetByIdAsync(model.ServicioId);
        if (servicio == null)
        {
            return (false, "El servicio seleccionado no existe.", null);
        }

        var cliente = await _clienteRepository.GetByIdAsync(model.ClienteId);
        if (cliente == null)
        {
            return (false, "Selecciona un cliente válido.", null);
        }

        var fin = model.FechaHoraInicio.AddMinutes(servicio.DuracionMinutos);

        if (!await _disponibilidadService.EstaDisponibleAsync(model.EmpleadoId, model.FechaHoraInicio, fin))
        {
            return (false, "El especialista seleccionado ya no está disponible en ese horario. Elige otro horario o especialista.", null);
        }

        var cita = new Cita
        {
            ClienteId = model.ClienteId,
            EmpleadoId = model.EmpleadoId,
            ServicioId = model.ServicioId,
            FechaHoraInicio = model.FechaHoraInicio,
            FechaHoraFin = fin,
            Estado = EstadoCita.Pendiente,
            Notas = model.Notas?.Trim(),
            FechaCreacion = DateTime.Now
        };

        await _repository.AddAsync(cita);
        await _repository.SaveChangesAsync();

        return (true, null, cita.Id);
    }

    public async Task<(bool Success, string? Error)> ActualizarAsync(CitaFormViewModel model)
    {
        var cita = await _repository.GetByIdAsync(model.Id);
        if (cita == null)
        {
            return (false, "Cita no encontrada.");
        }

        if (cita.Estado is EstadoCita.Completada or EstadoCita.Cancelada)
        {
            return (false, "No se puede modificar una cita completada o cancelada.");
        }

        var servicio = await _servicioRepository.GetByIdAsync(model.ServicioId);
        if (servicio == null)
        {
            return (false, "El servicio seleccionado no existe.");
        }

        var fin = model.FechaHoraInicio.AddMinutes(servicio.DuracionMinutos);

        if (!await _disponibilidadService.EstaDisponibleAsync(model.EmpleadoId, model.FechaHoraInicio, fin, cita.Id))
        {
            return (false, "El especialista seleccionado no está disponible en ese horario.");
        }

        cita.ClienteId = model.ClienteId;
        cita.EmpleadoId = model.EmpleadoId;
        cita.ServicioId = model.ServicioId;
        cita.FechaHoraInicio = model.FechaHoraInicio;
        cita.FechaHoraFin = fin;
        cita.Notas = model.Notas?.Trim();

        _repository.Update(cita);
        await _repository.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ReagendarAsync(int citaId, DateTime nuevoInicio)
    {
        var cita = await _repository.GetByIdAsync(citaId);
        if (cita == null)
        {
            return (false, "Cita no encontrada.");
        }

        if (cita.Estado is EstadoCita.Completada or EstadoCita.Cancelada)
        {
            return (false, "No se puede reagendar una cita completada o cancelada.");
        }

        var duracion = cita.FechaHoraFin - cita.FechaHoraInicio;
        var nuevoFin = nuevoInicio.Add(duracion);

        if (!await _disponibilidadService.EstaDisponibleAsync(cita.EmpleadoId, nuevoInicio, nuevoFin, cita.Id))
        {
            return (false, "El especialista no está disponible en el nuevo horario.");
        }

        cita.FechaHoraInicio = nuevoInicio;
        cita.FechaHoraFin = nuevoFin;

        _repository.Update(cita);
        await _repository.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> CambiarEstadoAsync(int citaId, EstadoCita nuevoEstado, int? empleadoIdSolicitante = null)
    {
        var cita = await _repository.GetByIdAsync(citaId);
        if (cita == null)
        {
            return (false, "Cita no encontrada.");
        }

        if (empleadoIdSolicitante.HasValue && cita.EmpleadoId != empleadoIdSolicitante.Value)
        {
            return (false, "No puedes modificar una cita que no es tuya.");
        }

        if (!TransicionValida(cita.Estado, nuevoEstado))
        {
            return (false, $"No se puede pasar de \"{cita.Estado}\" a \"{nuevoEstado}\".");
        }

        cita.Estado = nuevoEstado;
        _repository.Update(cita);
        await _repository.SaveChangesAsync();

        // Cuando la cita queda "Completada" ya está lista para el módulo de
        // Facturación (generar factura a partir de Cliente + Servicio + Precio).
        return (true, null);
    }

    public Task<List<Cliente>> BuscarClientesAsync(string term) => _clienteRepository.BuscarActivosAsync(term);

    private static bool TransicionValida(EstadoCita actual, EstadoCita nuevo)
    {
        if (actual == nuevo)
        {
            return true;
        }

        return actual switch
        {
            EstadoCita.Pendiente => nuevo is EstadoCita.Confirmada or EstadoCita.Cancelada,
            EstadoCita.Confirmada => nuevo is EstadoCita.EnProceso or EstadoCita.Cancelada or EstadoCita.NoAsistio,
            EstadoCita.EnProceso => nuevo is EstadoCita.Completada or EstadoCita.Cancelada,
            _ => false // Completada, Cancelada y NoAsistio son estados finales.
        };
    }

    private static DateTime RedondearProximaHora(DateTime valor)
    {
        var minutos = valor.Minute < 30 ? 30 : 60;
        return valor.Date.AddHours(valor.Hour).AddMinutes(minutos);
    }
}
