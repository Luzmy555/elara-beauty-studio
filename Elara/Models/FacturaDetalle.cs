namespace ElaraMVC.Models;

public class FacturaDetalle
{
    public int Id { get; set; }

    public int FacturaId { get; set; }
    public Factura? Factura { get; set; }

    // Exactamente uno de ServicioId/ProductoId tiene valor: una línea es de
    // servicio o de producto, nunca ambas. Se valida en el servicio, no acá.
    public int? ServicioId { get; set; }
    public Servicio? Servicio { get; set; }

    public int? ProductoId { get; set; }
    public Producto? Producto { get; set; }

    // Quién realizó el servicio o vendió el producto. Obligatorio a nivel de
    // formulario para líneas de servicio; opcional para líneas de producto
    // (una recepcionista puede vender un esmalte sin que medie un especialista).
    public int? EmpleadoId { get; set; }
    public Empleado? Empleado { get; set; }

    public int Cantidad { get; set; } = 1;

    // Copiado del precio vigente (Servicio.Precio o Producto.PrecioVenta) al
    // emitir, pero editable por quien factura; nunca se recalcula solo.
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }

    // Comisión de esta línea, calculada con el % vigente del empleado sobre
    // su parte del total ya con el descuento general de la factura prorrateado.
    // Queda en 0 si la línea no tiene EmpleadoId asignado.
    public decimal ComisionEmpleado { get; set; }

    public ICollection<Devolucion> Devoluciones { get; set; } = new List<Devolucion>();
}
