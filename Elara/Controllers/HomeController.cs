using System.Diagnostics;
using ElaraMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ElaraMVC.Models;

namespace ElaraMVC.Controllers;

[Authorize(Roles = "Administrador,Recepcionista")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IReporteService _reporteService;

    public HomeController(ILogger<HomeController> logger, IReporteService reporteService)
    {
        _logger = logger;
        _reporteService = reporteService;
    }

    public async Task<IActionResult> Index()
    {
        var dashboard = await _reporteService.ObtenerDashboardAsync();
        return View(dashboard);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
