using System.ComponentModel.DataAnnotations;

namespace ElaraMVC.ViewModels;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Correo inválido.")]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;
}
