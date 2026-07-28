using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ElaraMVC.ViewModels;

public class ClienteFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [StringLength(150, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Nombre completo")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [RegularExpression(@"^\+?[0-9\s\-\(\)]{7,20}$", ErrorMessage = "Formato de teléfono inválido.")]
    [Display(Name = "Teléfono")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Correo inválido.")]
    [StringLength(150)]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Fecha de nacimiento")]
    public DateTime? FechaNacimiento { get; set; }

    [StringLength(500, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Alergias o condiciones especiales")]
    public string? Alergias { get; set; }

    [StringLength(500, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Preferencias")]
    public string? Preferencias { get; set; }

    [Display(Name = "Foto de perfil")]
    public IFormFile? Foto { get; set; }

    public string? FotoActualUrl { get; set; }

    public bool Activo { get; set; } = true;
}
