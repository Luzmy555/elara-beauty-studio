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
    public List<Factura> Facturas { get; set; } = new();
}
