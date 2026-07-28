using ElaraMVC.Models;
using ElaraMVC.Repositories.Interfaces;
using ElaraMVC.Services.Interfaces;
using ElaraMVC.ViewModels;

namespace ElaraMVC.Services;

public class ServicioService : IServicioService
{
    private readonly IServicioRepository _repository;
    private readonly IFotoUploadService _fotoUploadService;

    public ServicioService(IServicioRepository repository, IFotoUploadService fotoUploadService)
    {
        _repository = repository;
        _fotoUploadService = fotoUploadService;
    }

    public Task<List<Servicio>> ListarAsync() => _repository.GetAllAsync();

    public Task<Servicio?> ObtenerPorIdAsync(int id) => _repository.GetByIdAsync(id);

    public async Task<(bool Success, string? Error)> CrearAsync(ServicioFormViewModel model)
    {
        var servicio = new Servicio
        {
            Nombre = model.Nombre.Trim(),
            CategoriaServicioId = model.CategoriaServicioId,
            Descripcion = model.Descripcion.Trim(),
            DuracionMinutos = model.DuracionMinutos,
            Precio = model.Precio,
            Activo = true
        };

        if (model.Imagen != null)
        {
            var (success, pathOrError) = await _fotoUploadService.GuardarAsync(model.Imagen, "servicios");
            if (!success)
            {
                return (false, pathOrError);
            }

            servicio.ImagenUrl = pathOrError;
        }

        await _repository.AddAsync(servicio);
        await _repository.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ActualizarAsync(ServicioFormViewModel model)
    {
        var servicio = await _repository.GetByIdAsync(model.Id);
        if (servicio == null)
        {
            return (false, "Servicio no encontrado.");
        }

        servicio.Nombre = model.Nombre.Trim();
        servicio.CategoriaServicioId = model.CategoriaServicioId;
        servicio.Descripcion = model.Descripcion.Trim();
        servicio.DuracionMinutos = model.DuracionMinutos;
        servicio.Precio = model.Precio;

        if (model.Imagen != null)
        {
            var (success, pathOrError) = await _fotoUploadService.GuardarAsync(model.Imagen, "servicios");
            if (!success)
            {
                return (false, pathOrError);
            }

            _fotoUploadService.Eliminar(servicio.ImagenUrl);
            servicio.ImagenUrl = pathOrError;
        }

        _repository.Update(servicio);
        await _repository.SaveChangesAsync();
        return (true, null);
    }

    public async Task<bool> CambiarEstadoAsync(int id)
    {
        var servicio = await _repository.GetByIdAsync(id);
        if (servicio == null)
        {
            return false;
        }

        servicio.Activo = !servicio.Activo;
        _repository.Update(servicio);
        await _repository.SaveChangesAsync();
        return true;
    }
}
