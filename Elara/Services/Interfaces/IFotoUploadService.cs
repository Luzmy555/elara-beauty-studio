using Microsoft.AspNetCore.Http;

namespace ElaraMVC.Services.Interfaces;

public interface IFotoUploadService
{
    Task<(bool Success, string PathOrError)> GuardarAsync(IFormFile foto, string subcarpeta);
    void Eliminar(string? rutaRelativa);
}
