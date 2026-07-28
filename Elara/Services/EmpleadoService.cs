using System.Security.Cryptography;
using ElaraMVC.Models;
using ElaraMVC.Repositories.Interfaces;
using ElaraMVC.Services.Interfaces;
using ElaraMVC.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace ElaraMVC.Services;

public class EmpleadoService : IEmpleadoService
{
    private const string RolEspecialista = "Especialista";

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

    private readonly IEmpleadoRepository _repository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFotoUploadService _fotoUploadService;

    public EmpleadoService(
        IEmpleadoRepository repository,
        UserManager<ApplicationUser> userManager,
        IFotoUploadService fotoUploadService)
    {
        _repository = repository;
        _userManager = userManager;
        _fotoUploadService = fotoUploadService;
    }

    public Task<List<Empleado>> ListarAsync() => _repository.GetAllAsync();

    public Task<Empleado?> ObtenerPorIdAsync(int id) => _repository.GetByIdAsync(id);

    public Task<Empleado?> ObtenerPorUsuarioAsync(string applicationUserId) =>
        _repository.GetByApplicationUserIdAsync(applicationUserId);

    public async Task<EmpleadoFormViewModel> ConstruirFormularioNuevoAsync()
    {
        return new EmpleadoFormViewModel
        {
            EspecialidadesDisponibles = await _repository.GetEspecialidadesAsync(),
            Horarios = ConstruirHorarioBase()
        };
    }

    public async Task<EmpleadoFormViewModel?> ConstruirFormularioEdicionAsync(int id)
    {
        var empleado = await _repository.GetByIdAsync(id);
        if (empleado == null)
        {
            return null;
        }

        var horarioBase = ConstruirHorarioBase();
        foreach (var dia in horarioBase)
        {
            var existente = empleado.Horarios.FirstOrDefault(h => h.DiaSemana == dia.DiaSemana);
            if (existente != null)
            {
                dia.Trabaja = existente.Trabaja;
                dia.HoraInicio = existente.HoraInicio;
                dia.HoraFin = existente.HoraFin;
            }
        }

        return new EmpleadoFormViewModel
        {
            Id = empleado.Id,
            NombreCompleto = empleado.NombreCompleto,
            Telefono = empleado.Telefono,
            Email = empleado.Email,
            ComisionPorcentaje = empleado.ComisionPorcentaje,
            Estado = empleado.Estado,
            FotoActualUrl = empleado.FotoUrl,
            EsEdicion = true,
            EspecialidadesSeleccionadas = empleado.EmpleadoEspecialidades.Select(ee => ee.EspecialidadId).ToList(),
            EspecialidadesDisponibles = await _repository.GetEspecialidadesAsync(),
            Horarios = horarioBase
        };
    }

    public async Task<(bool Success, string? Error, string? PasswordTemporal)> CrearAsync(EmpleadoFormViewModel model)
    {
        if (await _repository.EmailExistsAsync(model.Email, null))
        {
            return (false, "Ya existe un empleado registrado con este correo.", null);
        }

        if (await _userManager.FindByEmailAsync(model.Email) != null)
        {
            return (false, "Ya existe una cuenta de acceso con este correo.", null);
        }

        // La foto se valida y guarda ANTES de crear la cuenta de Identity para
        // no dejar un usuario huérfano si la imagen es inválida.
        string? fotoUrl = null;
        if (model.Foto != null)
        {
            var (success, pathOrError) = await _fotoUploadService.GuardarAsync(model.Foto, "empleados");
            if (!success)
            {
                return (false, pathOrError, null);
            }

            fotoUrl = pathOrError;
        }

        var passwordTemporal = GenerarPasswordTemporal();

        var usuario = new ApplicationUser
        {
            UserName = model.Email.Trim(),
            Email = model.Email.Trim(),
            FullName = model.NombreCompleto.Trim(),
            EmailConfirmed = true,
            MustChangePassword = true
        };

        var resultadoUsuario = await _userManager.CreateAsync(usuario, passwordTemporal);
        if (!resultadoUsuario.Succeeded)
        {
            _fotoUploadService.Eliminar(fotoUrl);
            var errores = string.Join(" ", resultadoUsuario.Errors.Select(e => e.Description));
            return (false, errores, null);
        }

        await _userManager.AddToRoleAsync(usuario, RolEspecialista);

        var empleado = new Empleado
        {
            ApplicationUserId = usuario.Id,
            NombreCompleto = model.NombreCompleto.Trim(),
            Telefono = model.Telefono.Trim(),
            Email = model.Email.Trim(),
            ComisionPorcentaje = model.ComisionPorcentaje,
            Estado = EstadoEmpleado.Activo,
            FechaRegistro = DateTime.Now,
            FotoUrl = fotoUrl
        };

        AsignarEspecialidades(empleado, model.EspecialidadesSeleccionadas);
        AsignarHorarios(empleado, model.Horarios);

        await _repository.AddAsync(empleado);
        await _repository.SaveChangesAsync();

        return (true, null, passwordTemporal);
    }

    public async Task<(bool Success, string? Error)> ActualizarAsync(EmpleadoFormViewModel model)
    {
        var empleado = await _repository.GetByIdAsync(model.Id);
        if (empleado == null)
        {
            return (false, "Empleado no encontrado.");
        }

        empleado.NombreCompleto = model.NombreCompleto.Trim();
        empleado.Telefono = model.Telefono.Trim();
        empleado.ComisionPorcentaje = model.ComisionPorcentaje;
        // El correo no se edita aquí: es el nombre de usuario de su cuenta de Identity.

        if (model.Foto != null)
        {
            var (success, pathOrError) = await _fotoUploadService.GuardarAsync(model.Foto, "empleados");
            if (!success)
            {
                return (false, pathOrError);
            }

            _fotoUploadService.Eliminar(empleado.FotoUrl);
            empleado.FotoUrl = pathOrError;
        }

        empleado.EmpleadoEspecialidades.Clear();
        AsignarEspecialidades(empleado, model.EspecialidadesSeleccionadas);

        empleado.Horarios.Clear();
        AsignarHorarios(empleado, model.Horarios);

        _repository.Update(empleado);
        await _repository.SaveChangesAsync();

        var usuario = await _userManager.FindByIdAsync(empleado.ApplicationUserId);
        if (usuario != null && usuario.FullName != empleado.NombreCompleto)
        {
            usuario.FullName = empleado.NombreCompleto;
            await _userManager.UpdateAsync(usuario);
        }

        return (true, null);
    }

    public async Task<bool> CambiarEstadoAsync(int id, EstadoEmpleado nuevoEstado)
    {
        var empleado = await _repository.GetByIdAsync(id);
        if (empleado == null)
        {
            return false;
        }

        empleado.Estado = nuevoEstado;
        _repository.Update(empleado);
        await _repository.SaveChangesAsync();

        // Solo "Inactivo" bloquea el inicio de sesión; "Vacaciones" es un estado
        // informativo/de agenda, el especialista puede seguir entrando al sistema.
        var usuario = await _userManager.FindByIdAsync(empleado.ApplicationUserId);
        if (usuario != null)
        {
            if (nuevoEstado == EstadoEmpleado.Inactivo)
            {
                await _userManager.SetLockoutEnabledAsync(usuario, true);
                await _userManager.SetLockoutEndDateAsync(usuario, DateTimeOffset.MaxValue);
            }
            else
            {
                await _userManager.SetLockoutEndDateAsync(usuario, null);
            }
        }

        return true;
    }

    public Task<decimal> CalcularComisionGanadaAsync(int empleadoId, DateTime desde, DateTime hasta)
    {
        // TODO: cuando exista el módulo de Facturación, sumar aquí
        // (Total del servicio × ComisionPorcentaje / 100) de cada factura de
        // este empleado dentro del rango [desde, hasta] y retornar el total.
        return Task.FromResult(0m);
    }

    private static List<HorarioDiaViewModel> ConstruirHorarioBase()
    {
        return DiasSemana
            .Select(d => new HorarioDiaViewModel { DiaSemana = d.Dia, NombreDia = d.Nombre, Trabaja = false })
            .ToList();
    }

    private static void AsignarEspecialidades(Empleado empleado, IEnumerable<int> especialidadIds)
    {
        foreach (var especialidadId in especialidadIds.Distinct())
        {
            empleado.EmpleadoEspecialidades.Add(new EmpleadoEspecialidad { EspecialidadId = especialidadId });
        }
    }

    private static void AsignarHorarios(Empleado empleado, IEnumerable<HorarioDiaViewModel> horarios)
    {
        foreach (var dia in horarios)
        {
            empleado.Horarios.Add(new HorarioTrabajo
            {
                DiaSemana = dia.DiaSemana,
                Trabaja = dia.Trabaja,
                HoraInicio = dia.Trabaja ? dia.HoraInicio : null,
                HoraFin = dia.Trabaja ? dia.HoraFin : null
            });
        }
    }

    private static string GenerarPasswordTemporal()
    {
        const string mayusculas = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string minusculas = "abcdefghijkmnpqrstuvwxyz";
        const string digitos = "23456789";
        const string especiales = "!@#$%";
        const string todos = mayusculas + minusculas + digitos + especiales;

        var resultado = new char[12];
        resultado[0] = mayusculas[RandomNumberGenerator.GetInt32(mayusculas.Length)];
        resultado[1] = minusculas[RandomNumberGenerator.GetInt32(minusculas.Length)];
        resultado[2] = digitos[RandomNumberGenerator.GetInt32(digitos.Length)];
        resultado[3] = especiales[RandomNumberGenerator.GetInt32(especiales.Length)];

        for (var i = 4; i < resultado.Length; i++)
        {
            resultado[i] = todos[RandomNumberGenerator.GetInt32(todos.Length)];
        }

        for (var i = resultado.Length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (resultado[i], resultado[j]) = (resultado[j], resultado[i]);
        }

        return new string(resultado);
    }
}
