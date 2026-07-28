namespace ElaraMVC.Models;

public class Factura
{
    public int Id { get; set; }

    public int CitaId { get; set; }
    public Cita? Cita { get; set; }

    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public int EmpleadoId { get; set; }
    public Empleado? Empleado { get; set; }

    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public string? DescuentoJustificacion { get; set; }
    public decimal Total { get; set; }

    public MetodoPago MetodoPago { get; set; }
    public EstadoFactura Estado { get; set; } = EstadoFactura.Pagada;

    public DateTime FechaEmision { get; set; } = DateTime.Now;

    // Comisión calculada al emitir la factura con el % vigente en ese momento
    // en el perfil del empleado (Empleado.ComisionPorcentaje). No se recalcula
    // retroactivamente si el % del empleado cambia después.
    public decimal ComisionEmpleado { get; set; }
}
