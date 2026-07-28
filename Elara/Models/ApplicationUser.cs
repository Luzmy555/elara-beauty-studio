using Microsoft.AspNetCore.Identity;

namespace ElaraMVC.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // true cuando la cuenta se creó con una contraseña temporal (ej. módulo
    // Empleados) y debe cambiarla antes de usar el resto del sistema.
    public bool MustChangePassword { get; set; } = false;

    // Preferencia de tema del usuario (Claro/Oscuro), persistida por cuenta
    // para que se recuerde al iniciar sesión desde cualquier dispositivo.
    public bool TemaOscuro { get; set; } = false;
}
