using ElaraMVC.Models;

namespace ElaraMVC.ViewModels;

public class ReporteCajaViewModel
{
    public DateTime Fecha { get; set; }
    public decimal TotalEfectivo { get; set; }
    public decimal TotalTarjeta { get; set; }
    public decimal TotalTransferencia { get; set; }
    public decimal TotalGeneral => TotalEfectivo + TotalTarjeta + TotalTransferencia;
    public decimal TotalPendiente { get; set; }
    public int CantidadFacturas { get; set; }
    public int CantidadDesdeCita { get; set; }
    public int CantidadVentaRapida { get; set; }
    public List<Factura> Facturas { get; set; } = new();

    // "Egresos" del día: devoluciones procesadas (no gastos operativos, que
    // el sistema no rastrea).
    public decimal TotalDevoluciones { get; set; }
    public decimal TotalNeto => TotalGeneral - TotalDevoluciones;
    public List<Devolucion> Devoluciones { get; set; } = new();
}
