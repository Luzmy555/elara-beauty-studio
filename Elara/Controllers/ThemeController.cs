using ElaraMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElaraMVC.Controllers;

// Endpoint liviano que usa el toggle sol/luna del navbar (fetch AJAX) para
// persistir la preferencia de tema del usuario autenticado en la BD.
[Authorize]
public class ThemeController : Controller
{
    private readonly IThemeService _themeService;

    public ThemeController(IThemeService themeService)
    {
        _themeService = themeService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Establecer(bool esOscuro)
    {
        var success = await _themeService.EstablecerTemaAsync(User, esOscuro);
        return Json(new { success });
    }
}
