using System.ComponentModel.DataAnnotations;
using ElaraMVC.Models;

namespace ElaraMVC.ViewModels;

public class MovimientoFormViewModel
{
    [Required]
    public int ProductoId { get; set; }

    [Required(ErrorMessage = "Selecciona el tipo de movimiento.")]
    public TipoMovimiento Tipo { get; set; }

    [Range(0.01, 100000, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    public decimal Cantidad { get; set; }

    [Required(ErrorMessage = "Indica el motivo del movimiento.")]
    [StringLength(300, ErrorMessage = "Máximo {1} caracteres.")]
    public string Motivo { get; set; } = string.Empty;
}
