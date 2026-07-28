using System.ComponentModel.DataAnnotations;

namespace ElaraMVC.ViewModels;

public class CategoriaServicioFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Nombre de la categoría")]
    public string Nombre { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;
}
