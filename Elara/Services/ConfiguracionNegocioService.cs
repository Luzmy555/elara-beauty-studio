using ElaraMVC.Models;
using ElaraMVC.Repositories.Interfaces;
using ElaraMVC.Services.Interfaces;
using ElaraMVC.ViewModels;

namespace ElaraMVC.Services;

public class ConfiguracionNegocioService : IConfiguracionNegocioService
{
    private static readonly (DayOfWeek Dia, string Nombre)[] DiasSemana =
    {
        (DayOfWeek.Monday, "Lunes"),
        (DayOfWeek.Tuesday, "Martes"),
        (DayOfWeek.Wednesday, "Miércoles"),
        (DayOfWeek.Thursday, "Jueves"),
        (DayOfWeek.Friday, "Viernes"),
        (DayOfWeek.Saturday, "Sábado"),
        (DayOfWeek.Sunday, "Domingo")
    };

    private readonly IConfiguracionNegocioRepository _repository;
    private readonly IFotoUploadService _fotoUploadService;

    public ConfiguracionNegocioService(IConfiguracionNegocioRepository repository, IFotoUploadService fotoUploadService)
    {
        _repository = repository;
        _fotoUploadService = fotoUploadService;
    }

    // Fila única de configuración: si aún no existe (primera vez que se pide),
    // se crea con los 7 días cerrados por defecto y el admin la completa.
    public async Task<ConfiguracionNegocio> ObtenerAsync()
    {
        var configuracion = await _repository.ObtenerAsync();
        if (configuracion != null)
        {
            return configuracion;
        }

        configuracion = new ConfiguracionNegocio();
        foreach (var dia in DiasSemana)
        {
            configuracion.Horarios.Add(new HorarioNegocio { DiaSemana = dia.Dia, Abierto = false });
        }

        await _repository.AddAsync(configuracion);
        await _repository.SaveChangesAsync();
        return configuracion;
    }

    public async Task<ConfiguracionNegocioViewModel> ConstruirFormularioAsync()
    {
        var configuracion = await ObtenerAsync();

        var horarios = DiasSemana.Select(d =>
        {
            var existente = configuracion.Horarios.FirstOrDefault(h => h.DiaSemana == d.Dia);
            return new HorarioDiaNegocioViewModel
            {
                DiaSemana = d.Dia,
                NombreDia = d.Nombre,
                Abierto = existente?.Abierto ?? false,
                HoraApertura = existente?.HoraApertura,
                HoraCierre = existente?.HoraCierre
            };
        }).ToList();

        return new ConfiguracionNegocioViewModel
        {
            NombreSalon = configuracion.NombreSalon,
            LogoActualUrl = configuracion.LogoUrl,
            Direccion = configuracion.Direccion,
            Telefono = configuracion.Telefono,
            EmailContacto = configuracion.EmailContacto,
            Moneda = configuracion.Moneda,
            Horarios = horarios
        };
    }

    public async Task<(bool Success, string? Error)> ActualizarAsync(ConfiguracionNegocioViewModel model)
    {
        var configuracion = await ObtenerAsync();

        configuracion.NombreSalon = model.NombreSalon.Trim();
        configuracion.Direccion = model.Direccion?.Trim();
        configuracion.Telefono = model.Telefono?.Trim();
        configuracion.EmailContacto = model.EmailContacto?.Trim();
        configuracion.Moneda = model.Moneda;

        if (model.Logo != null)
        {
            var (success, pathOrError) = await _fotoUploadService.GuardarAsync(model.Logo, "negocio");
            if (!success)
            {
                return (false, pathOrError);
            }

            _fotoUploadService.Eliminar(configuracion.LogoUrl);
            configuracion.LogoUrl = pathOrError;
        }

        foreach (var diaVm in model.Horarios)
        {
            var horario = configuracion.Horarios.FirstOrDefault(h => h.DiaSemana == diaVm.DiaSemana);
            if (horario == null)
            {
                horario = new HorarioNegocio { DiaSemana = diaVm.DiaSemana };
                configuracion.Horarios.Add(horario);
            }

            horario.Abierto = diaVm.Abierto;
            horario.HoraApertura = diaVm.Abierto ? diaVm.HoraApertura : null;
            horario.HoraCierre = diaVm.Abierto ? diaVm.HoraCierre : null;
        }

        _repository.Update(configuracion);
        await _repository.SaveChangesAsync();
        return (true, null);
    }
}
