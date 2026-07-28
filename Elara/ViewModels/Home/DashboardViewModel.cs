namespace ElaraMVC.ViewModels;

public class DashboardViewModel
{
    public int CitasHoy { get; set; }
    public decimal IngresosHoy { get; set; }
    public int ClientesNuevosSemana { get; set; }
    public int ProductosPorReabastecer { get; set; }
    public List<ProximaCitaViewModel> ProximasCitasHoy { get; set; } = new();
}
