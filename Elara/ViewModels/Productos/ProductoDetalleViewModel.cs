using ElaraMVC.Models;

namespace ElaraMVC.ViewModels;

public class ProductoDetalleViewModel
{
    public Producto Producto { get; set; } = null!;
    public List<MovimientoInventario> Movimientos { get; set; } = new();
}
