using System.Text;
using ElaraMVC.Models;

namespace ElaraMVC.Services;

// Arma links de "click to chat" de WhatsApp (wa.me): gratis, sin API ni
// cuenta de desarrollador, pero requiere que el usuario presione "Enviar" y
// no permite adjuntar archivos, solo texto pre-cargado.
public static class WhatsAppLinkHelper
{
    // Mensaje con tono cálido/femenino acorde a la marca (ver Elara.styles.css):
    // se usa al compartir una factura por WhatsApp desde Facturas/Details.
    public static string ConstruirMensajeFactura(Factura factura, string nombreCliente)
    {
        var sb = new StringBuilder();
        sb.Append("✨ ¡Hola, ").Append(nombreCliente).Append("! ✨\n\n");
        sb.Append("Gracias por tu visita. A continuación, el detalle de tu factura:\n\n");
        sb.Append("🧾 Factura N.º: ").Append(factura.NumeroFactura).Append('\n');
        sb.Append("📅 Fecha: ").Append(factura.FechaEmision.ToString("dd/MM/yyyy")).Append("\n\n");

        sb.Append("Detalle de la compra:\n");
        foreach (var detalle in factura.FacturaDetalles)
        {
            var nombre = detalle.Servicio?.Nombre ?? detalle.Producto?.Nombre ?? "Ítem";
            if (detalle.Cantidad > 1)
            {
                nombre += $" (x{detalle.Cantidad})";
            }

            var puntos = new string('.', Math.Max(3, 26 - nombre.Length));
            sb.Append("• ").Append(nombre).Append(' ').Append(puntos)
              .Append(" RD$ ").Append(detalle.Subtotal.ToString("N2")).Append('\n');
        }

        sb.Append('\n');
        sb.Append("━━━━━━━━━━━━━━━━━━\n");
        sb.Append("💰 Total: RD$ ").Append(factura.Total.ToString("N2")).Append('\n');
        sb.Append("━━━━━━━━━━━━━━━━━━\n\n");
        sb.Append("¡Gracias por elegirnos! 💖\n");
        sb.Append("Será un placer atenderte nuevamente. ¡Te esperamos pronto! 🌸");

        return sb.ToString();
    }

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
