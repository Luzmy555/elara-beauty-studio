using System.ComponentModel.DataAnnotations;
using ElaraMVC.Models;

namespace ElaraMVC.ViewModels;

public class ProductoFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Display(Name = "Categoría")]
    public CategoriaProducto Categoria { get; set; }

    [Required(ErrorMessage = "La marca es obligatoria.")]
    [StringLength(100)]
    [Display(Name = "Marca")]
    public string Marca { get; set; } = string.Empty;

    // Solo se usa al crear (stock inicial); en edición se muestra de solo
    // lectura y los cambios reales pasan por "Registrar movimiento".
    [Range(0, 100000, ErrorMessage = "La cantidad no puede ser negativa.")]
    [Display(Name = "Cantidad actual")]
    public decimal CantidadActual { get; set; }

    [Range(0.01, 100000, ErrorMessage = "La cantidad mínima debe ser mayor a 0.")]
    [Display(Name = "Cantidad mínima")]
    public decimal CantidadMinima { get; set; }

    [Display(Name = "Unidad de medida")]
    public UnidadMedida UnidadMedida { get; set; }

    [Range(0.01, 1000000, ErrorMessage = "El precio de costo debe ser mayor a 0.")]
    [Display(Name = "Precio de costo")]
    public decimal PrecioCosto { get; set; }

    [Range(0.01, 1000000, ErrorMessage = "El precio de venta debe ser mayor a 0.")]
    [Display(Name = "Precio de venta")]
    public decimal PrecioVenta { get; set; }

    [StringLength(150)]
    [Display(Name = "Proveedor")]
    public string? Proveedor { get; set; }

    public DateTime? FechaUltimaCompra { get; set; }

    public bool EsEdicion { get; set; }
}
