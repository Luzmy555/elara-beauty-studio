using ElaraMVC.Models;
using ElaraMVC.Repositories.Interfaces;
using ElaraMVC.Services.Interfaces;
using ElaraMVC.ViewModels;

namespace ElaraMVC.Services;

public class CategoriaServicioService : ICategoriaServicioService
{
    private readonly ICategoriaServicioRepository _repository;

    public CategoriaServicioService(ICategoriaServicioRepository repository)
    {
        _repository = repository;
    }

    public Task<List<CategoriaServicio>> ListarAsync() => _repository.GetAllAsync();

    public Task<List<CategoriaServicio>> ListarActivasAsync() => _repository.GetActivasAsync();

    public Task<CategoriaServicio?> ObtenerPorIdAsync(int id) => _repository.GetByIdAsync(id);

    public async Task<(bool Success, string? Error)> CrearAsync(CategoriaServicioFormViewModel model)
    {
        var nombre = model.Nombre.Trim();
        if (await _repository.ExisteNombreAsync(nombre, null))
        {
            return (false, "Ya existe una categoría con este nombre.");
        }

        await _repository.AddAsync(new CategoriaServicio { Nombre = nombre, Activo = true });
        await _repository.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ActualizarAsync(CategoriaServicioFormViewModel model)
    {
        var categoria = await _repository.GetByIdAsync(model.Id);
        if (categoria == null)
        {
            return (false, "Categoría no encontrada.");
        }

        var nombre = model.Nombre.Trim();
        if (await _repository.ExisteNombreAsync(nombre, categoria.Id))
        {
            return (false, "Ya existe una categoría con este nombre.");
        }

        categoria.Nombre = nombre;
        _repository.Update(categoria);
        await _repository.SaveChangesAsync();
        return (true, null);
    }

    public async Task<bool> CambiarEstadoAsync(int id)
    {
        var categoria = await _repository.GetByIdAsync(id);
        if (categoria == null)
        {
            return false;
        }

        categoria.Activo = !categoria.Activo;
        _repository.Update(categoria);
        await _repository.SaveChangesAsync();
        return true;
    }

    public async Task<(bool Success, string? Error)> EliminarAsync(int id)
    {
        var categoria = await _repository.GetByIdAsync(id);
        if (categoria == null)
        {
            return (false, "Categoría no encontrada.");
        }

        if (await _repository.TieneServiciosAsync(id))
        {
            return (false, "No se puede eliminar: hay servicios que usan esta categoría. Desactívala en su lugar.");
        }

        _repository.Remove(categoria);
        await _repository.SaveChangesAsync();
        return (true, null);
    }
}
