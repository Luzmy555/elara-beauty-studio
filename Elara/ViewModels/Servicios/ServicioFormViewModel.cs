using System.ComponentModel.DataAnnotations;
using ElaraMVC.Models;
using Microsoft.AspNetCore.Http;

namespace ElaraMVC.ViewModels;

public class ServicioFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Nombre del servicio")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecciona una categoría.")]
    [Display(Name = "Categoría")]
    public int CategoriaServicioId { get; set; }

    public List<CategoriaServicio> CategoriasDisponibles { get; set; } = new();

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(500, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Descripción")]
    public string Descripcion { get; set; } = string.Empty;

    [Range(1, 600, ErrorMessage = "La duración debe ser mayor a 0 minutos (máximo 600).")]
    [Display(Name = "Duración (minutos)")]
    public int DuracionMinutos { get; set; }

    [Range(0.01, 100000, ErrorMessage = "El precio debe ser mayor a 0.")]
    [Display(Name = "Precio")]
    public decimal Precio { get; set; }

    [Display(Name = "Imagen del servicio")]
    public IFormFile? Imagen { get; set; }
    public string? ImagenActualUrl { get; set; }

    public bool Activo { get; set; } = true;
}
