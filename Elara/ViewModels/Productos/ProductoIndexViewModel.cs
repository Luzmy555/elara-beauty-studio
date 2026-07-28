using ElaraMVC.Models;

namespace ElaraMVC.ViewModels;

public class ProductoIndexViewModel
{
    public List<Producto> Productos { get; set; } = new();
    public string? CategoriaSeleccionada { get; set; }
    public string? Term { get; set; }
    public int TotalBajoStock { get; set; }
}
