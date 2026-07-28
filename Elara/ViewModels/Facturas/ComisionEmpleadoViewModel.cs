namespace ElaraMVC.ViewModels;

public class ComisionEmpleadoViewModel
{
    public int EmpleadoId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public int CantidadServicios { get; set; }
    public decimal TotalFacturado { get; set; }
    public decimal TotalComision { get; set; }
}
