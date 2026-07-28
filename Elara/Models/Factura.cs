using System.ComponentModel.DataAnnotations.Schema;

namespace ElaraMVC.Models;

public class Factura
{
    public int Id { get; set; }

    // Correlativo mostrado al cliente (ej. "ELR-0001"). Se fija recién
    // después del insert, cuando ya se conoce el Id autoincremental.
    public string NumeroFactura { get; set; } = string.Empty;

    // Null cuando la factura viene de una venta rápida (walk-in) sin cita previa.
    public int? CitaId { get; set; }
    public Cita? Cita { get; set; }

    // Null cuando el cliente es "sin registrar" (walk-in que no quiso darse de alta).
    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    // Solo se usa cuando ClienteId es null: teléfono capturado a mano para
    // poder enviarle el link de WhatsApp sin crear un registro de Cliente.
    public string? ClienteTelefonoContacto { get; set; }

    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public string? DescuentoJustificacion { get; set; }
    public decimal Total { get; set; }

    public MetodoPago MetodoPago { get; set; }
    public EstadoFactura Estado { get; set; } = EstadoFactura.Pagada;

    // Solo aplica a MetodoPago.Efectivo: cuánto entregó el cliente físicamente,
    // para poder calcular la devuelta en caja. Null en pagos con tarjeta/transferencia.
    public decimal? MontoRecibido { get; set; }

    [NotMapped]
    public decimal? Devuelta => MontoRecibido.HasValue ? MontoRecibido.Value - Total : null;

    // Solo aplica a MetodoPago.Transferencia; se puede subir al emitir la
    // factura o después, desde el detalle. Opcional en ambos casos.
    public string? ComprobanteTransferenciaUrl { get; set; }

    public DateTime FechaEmision { get; set; } = DateTime.Now;

    // El empleado y la comisión viven por línea (FacturaDetalle), no aquí:
    // una misma factura puede tener varios especialistas distintos.
    public ICollection<FacturaDetalle> FacturaDetalles { get; set; } = new List<FacturaDetalle>();
}
