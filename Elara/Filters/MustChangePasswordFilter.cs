using ElaraMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ElaraMVC.Filters;

// Redirige a cualquier usuario autenticado con contraseña temporal pendiente
// (ver ApplicationUser.MustChangePassword) hacia Account/ChangePassword,
// sin importar a qué ruta intente entrar.
public class MustChangePasswordFilter : IAsyncActionFilter
{
    private readonly UserManager<ApplicationUser> _userManager;

    public MustChangePasswordFilter(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var httpContext = context.HttpContext;

        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            var controller = context.RouteData.Values["controller"]?.ToString();
            var action = context.RouteData.Values["action"]?.ToString();

            var esRutaPermitidaSinCambio = controller == "Account" &&
                (action == "ChangePassword" || action == "Logout");

            if (!esRutaPermitidaSinCambio)
            {
                var usuario = await _userManager.GetUserAsync(httpContext.User);
                if (usuario is { MustChangePassword: true })
                {
                    context.Result = new RedirectToActionResult("ChangePassword", "Account", null);
                    return;
                }
            }
        }

        await next();
    }
}
