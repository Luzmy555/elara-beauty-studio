namespace ElaraMVC.Services;

// Arma links de "click to chat" de WhatsApp (wa.me): gratis, sin API ni
// cuenta de desarrollador, pero requiere que el usuario presione "Enviar" y
// no permite adjuntar archivos, solo texto pre-cargado.
public static class WhatsAppLinkHelper
{
    // El salón opera en República Dominicana (numeración NANP, código de
    // país "1"): si el teléfono ya viene con código de país no se toca; si
    // son 10 dígitos locales se le antepone el "1".
    public static string LimpiarTelefono(string? telefono)
    {
        var digitos = new string((telefono ?? string.Empty).Where(char.IsDigit).ToArray());
        return digitos.Length == 10 ? "1" + digitos : digitos;
    }

    public static string? ConstruirLink(string? telefono, string mensaje)
    {
        var numero = LimpiarTelefono(telefono);
        return numero.Length == 0 ? null : $"https://wa.me/{numero}?text={Uri.EscapeDataString(mensaje)}";
    }
}
