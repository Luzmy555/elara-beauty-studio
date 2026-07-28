namespace ElaraMVC.Models;

public class FacturaDetalle
{
    public int Id { get; set; }

    public int FacturaId { get; set; }
    public Factura? Factura { get; set; }

    public int ServicioId { get; set; }
    public Servicio? Servicio { get; set; }

    // Quién realizó este servicio específico: distintas líneas de la misma
    // factura pueden tener especialistas distintos (ej. manicura con una,
    // cejas con otra).
    public int EmpleadoId { get; set; }
    public Empleado? Empleado { get; set; }

    public int Cantidad { get; set; } = 1;

    // Copiado de Servicio.Precio al emitir; nunca se toma del formulario.
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }

    // Comisión de esta línea, calculada con el % vigente del empleado sobre
    // su parte del total ya con el descuento general de la factura prorrateado.
    public decimal ComisionEmpleado { get; set; }
}
