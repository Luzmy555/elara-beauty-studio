namespace ElaraMVC.Models;

public class MovimientoInventario
{
    public int Id { get; set; }

    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }

    public TipoMovimiento Tipo { get; set; }
    public decimal Cantidad { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    public string Motivo { get; set; } = string.Empty;

    // Null salvo que el movimiento venga de una venta o devolución en
    // Facturación: enlaza el movimiento con la línea que lo originó para
    // trazabilidad. Los movimientos manuales del módulo de Inventario no lo usan.
    public int? FacturaDetalleId { get; set; }
    public FacturaDetalle? FacturaDetalle { get; set; }
}
