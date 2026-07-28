using System.Security.Claims;

namespace ElaraMVC.Services.Interfaces;

public interface IThemeService
{
    Task<bool> EsTemaOscuroAsync(ClaimsPrincipal principal);
    Task<bool> EstablecerTemaAsync(ClaimsPrincipal principal, bool esOscuro);
}
