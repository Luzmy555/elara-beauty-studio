using System.Security.Claims;
using ElaraMVC.Models;
using ElaraMVC.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace ElaraMVC.Services;

// La preferencia de tema se guarda en AspNetUsers.TemaOscuro (no en
// localStorage/cookies) para que se recuerde al entrar desde otro
// dispositivo. Usuarios anónimos (ej. pantalla de login) siempre ven claro.
public class ThemeService : IThemeService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ThemeService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> EsTemaOscuroAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        var usuario = await _userManager.GetUserAsync(principal);
        return usuario?.TemaOscuro ?? false;
    }

    public async Task<bool> EstablecerTemaAsync(ClaimsPrincipal principal, bool esOscuro)
    {
        var usuario = await _userManager.GetUserAsync(principal);
        if (usuario == null)
        {
            return false;
        }

        usuario.TemaOscuro = esOscuro;
        await _userManager.UpdateAsync(usuario);
        return true;
    }
}
