using System.ComponentModel.DataAnnotations;
using ElaraMVC.Models;

namespace ElaraMVC.ViewModels;

public class VentaRapidaLineaViewModel
{
    [Required(ErrorMessage = "Selecciona un servicio.")]
    public int ServicioId { get; set; }

    [Required(ErrorMessage = "Selecciona quién atendió.")]
    public int EmpleadoId { get; set; }

    [Range(1, 100, ErrorMessage = "La cantidad debe ser al menos 1.")]
    public int Cantidad { get; set; } = 1;

    // Se precarga con el precio de catálogo del servicio pero el recepcionista
    // puede editarlo (precio negociado, promoción puntual, etc.).
    [Range(0, 1000000, ErrorMessage = "El precio no puede ser negativo.")]
    public decimal PrecioUnitario { get; set; }
}

public class VentaRapidaViewModel : IValidatableObject
{
    // Null = "Cliente sin registrar / Walk-in".
    public int? ClienteId { get; set; }

    [StringLength(20, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Teléfono (para WhatsApp)")]
    public string? ClienteTelefonoContacto { get; set; }

    public List<VentaRapidaLineaViewModel> Lineas { get; set; } = new();

    [Range(0, 1000000, ErrorMessage = "El descuento no puede ser negativo.")]
    [Display(Name = "Descuento")]
    public decimal Descuento { get; set; }

    [StringLength(300, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Justificación del descuento")]
    public string? DescuentoJustificacion { get; set; }

    [Required(ErrorMessage = "Selecciona un método de pago.")]
    [Display(Name = "Método de pago")]
    public MetodoPago MetodoPago { get; set; } = MetodoPago.Efectivo;

    [Display(Name = "Estado")]
    public EstadoFactura Estado { get; set; } = EstadoFactura.Pagada;

    // Solo tiene sentido con MetodoPago.Efectivo; con tarjeta/transferencia se
    // ignora aunque venga cargado (no hay "devuelta" en esos métodos).
    [Range(0, 10000000, ErrorMessage = "El monto recibido no puede ser negativo.")]
    [Display(Name = "Monto recibido")]
    public decimal? MontoRecibido { get; set; }

    // Para repoblar los selectores si el formulario vuelve por un error de validación.
    public List<Cliente> ClientesDisponibles { get; set; } = new();
    public List<Empleado> EmpleadosDisponibles { get; set; } = new();
    public List<Servicio> ServiciosDisponibles { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Lineas == null || Lineas.Count == 0)
        {
            yield return new ValidationResult(
                "Agrega al menos un servicio a la venta.",
                new[] { nameof(Lineas) });
        }

        if (Descuento > 0 && string.IsNullOrWhiteSpace(DescuentoJustificacion))
        {
            yield return new ValidationResult(
                "Si aplicas un descuento, indica la justificación.",
                new[] { nameof(DescuentoJustificacion) });
        }

        if (MetodoPago == MetodoPago.Efectivo && MontoRecibido.HasValue && Lineas != null && Lineas.Count > 0)
        {
            var subtotal = Lineas.Sum(l => l.PrecioUnitario * l.Cantidad);
            var total = subtotal - Descuento;
            if (MontoRecibido.Value < total)
            {
                yield return new ValidationResult(
                    "El monto recibido es menor al total a cobrar.",
                    new[] { nameof(MontoRecibido) });
            }
        }
    }
}
