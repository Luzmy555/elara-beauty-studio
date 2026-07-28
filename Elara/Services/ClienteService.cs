using ElaraMVC.Models;
using ElaraMVC.Repositories.Interfaces;
using ElaraMVC.Services.Interfaces;
using ElaraMVC.ViewModels;

namespace ElaraMVC.Services;

public class ClienteService : IClienteService
{
    private const int PageSize = 10;

    private readonly IClienteRepository _repository;
    private readonly IFotoUploadService _fotoUploadService;

    public ClienteService(IClienteRepository repository, IFotoUploadService fotoUploadService)
    {
        _repository = repository;
        _fotoUploadService = fotoUploadService;
    }

    public async Task<ClienteIndexViewModel> BuscarAsync(string? term, int page)
    {
        page = page < 1 ? 1 : page;

        var (items, totalCount) = await _repository.GetPagedAsync(term, page, PageSize);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)PageSize);

        return new ClienteIndexViewModel
        {
            Clientes = items,
            SearchTerm = term ?? string.Empty,
            PageNumber = page,
            PageSize = PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    public Task<Cliente?> ObtenerPorIdAsync(int id)
    {
        return _repository.GetByIdAsync(id);
    }

    public async Task<(bool Success, string? Error)> CrearAsync(ClienteFormViewModel model)
    {
        if (await _repository.EmailExistsAsync(model.Email, null))
        {
            return (false, "Ya existe un cliente registrado con este correo.");
        }

        var cliente = new Cliente
        {
            NombreCompleto = model.NombreCompleto.Trim(),
            Telefono = model.Telefono.Trim(),
            Email = model.Email.Trim(),
            FechaNacimiento = model.FechaNacimiento,
            Alergias = model.Alergias?.Trim(),
            Preferencias = model.Preferencias?.Trim(),
            FechaRegistro = DateTime.Now,
            Activo = true
        };

        if (model.Foto != null)
        {
            var (success, pathOrError) = await _fotoUploadService.GuardarAsync(model.Foto, "clientes");
            if (!success)
            {
                return (false, pathOrError);
            }

            cliente.FotoUrl = pathOrError;
        }

        await _repository.AddAsync(cliente);
        await _repository.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ActualizarAsync(ClienteFormViewModel model)
    {
        var cliente = await _repository.GetByIdAsync(model.Id);
        if (cliente == null)
        {
            return (false, "Cliente no encontrado.");
        }

        if (await _repository.EmailExistsAsync(model.Email, model.Id))
        {
            return (false, "Ya existe otro cliente registrado con este correo.");
        }

        cliente.NombreCompleto = model.NombreCompleto.Trim();
        cliente.Telefono = model.Telefono.Trim();
        cliente.Email = model.Email.Trim();
        cliente.FechaNacimiento = model.FechaNacimiento;
        cliente.Alergias = model.Alergias?.Trim();
        cliente.Preferencias = model.Preferencias?.Trim();

        if (model.Foto != null)
        {
            var (success, pathOrError) = await _fotoUploadService.GuardarAsync(model.Foto, "clientes");
            if (!success)
            {
                return (false, pathOrError);
            }

            _fotoUploadService.Eliminar(cliente.FotoUrl);
            cliente.FotoUrl = pathOrError;
        }

        _repository.Update(cliente);
        await _repository.SaveChangesAsync();
        return (true, null);
    }

    public async Task<bool> CambiarEstadoAsync(int id)
    {
        var cliente = await _repository.GetByIdAsync(id);
        if (cliente == null)
        {
            return false;
        }

        cliente.Activo = !cliente.Activo;
        _repository.Update(cliente);
        await _repository.SaveChangesAsync();
        return true;
    }
}
