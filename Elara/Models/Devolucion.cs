namespace ElaraMVC.Models;

// Devolución de un producto ya facturado: solo aplica a líneas de producto
// (no tiene sentido "reingresar" un servicio ya realizado). Reintegra stock
// y registra la salida de dinero como comprobante imprimible.
public class Devolucion
{
    public int Id { get; set; }

    // Correlativo mostrado al cliente (ej. "DEV-0001"), mismo patrón que
    // Factura.NumeroFactura: se fija tras el insert, con el Id ya conocido.
    public string NumeroDevolucion { get; set; } = string.Empty;

    public int FacturaDetalleId { get; set; }
    public FacturaDetalle? FacturaDetalle { get; set; }

    public int Cantidad { get; set; }

    // Calculado automáticamente como (FacturaDetalle.Subtotal / Cantidad de
    // la línea) * Cantidad devuelta; no lo edita quien procesa la devolución.
    public decimal MontoReembolsado { get; set; }

    public string Motivo { get; set; } = string.Empty;

    // Se elige al procesar la devolución: no siempre coincide con el método
    // de la venta original (ej. vendió con tarjeta, reembolsa en efectivo).
    public MetodoPago MetodoReembolso { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Now;

    // Id del ApplicationUser (Administrador/Recepcionista) que la procesó.
    public string ProcesadoPorUserId { get; set; } = string.Empty;
}
