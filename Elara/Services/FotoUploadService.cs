using ElaraMVC.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ElaraMVC.Services;

public class FotoUploadService : IFotoUploadService
{
    private const long MaxBytes = 2 * 1024 * 1024;
    private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".webp" };

    private readonly IWebHostEnvironment _environment;

    public FotoUploadService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<(bool Success, string PathOrError)> GuardarAsync(IFormFile foto, string subcarpeta)
    {
        var extension = Path.GetExtension(foto.FileName).ToLowerInvariant();
        if (!ExtensionesPermitidas.Contains(extension))
        {
            return (false, "Formato de imagen no permitido. Usa JPG, PNG o WEBP.");
        }

        if (foto.Length > MaxBytes)
        {
            return (false, "La imagen no debe superar los 2 MB.");
        }

        var carpeta = Path.Combine(_environment.WebRootPath, "uploads", subcarpeta);
        Directory.CreateDirectory(carpeta);

        var nombreArchivo = $"{Guid.NewGuid()}{extension}";
        var rutaFisica = Path.Combine(carpeta, nombreArchivo);

        using (var stream = new FileStream(rutaFisica, FileMode.Create))
        {
            await foto.CopyToAsync(stream);
        }

        return (true, $"/uploads/{subcarpeta}/{nombreArchivo}");
    }

    public void Eliminar(string? rutaRelativa)
    {
        if (string.IsNullOrEmpty(rutaRelativa))
        {
            return;
        }

        var rutaFisica = Path.Combine(
            _environment.WebRootPath,
            rutaRelativa.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

        if (File.Exists(rutaFisica))
        {
            File.Delete(rutaFisica);
        }
    }
}
