namespace ElaraMVC.ViewModels;

// Un punto de una serie de tiempo mensual (ingresos, clientes nuevos, etc.).
public class SerieMensualViewModel
{
    public DateTime Periodo { get; set; }
    public string Etiqueta { get; set; } = string.Empty;
    public decimal Valor { get; set; }
}
