namespace ElaraMVC.ViewModels;

// Forma plana que consume el listado manual de citas (wwwroot/js/citas.js).
// System.Text.Json serializa las propiedades en camelCase por defecto en MVC.
public class CitaEventoViewModel
{
    public int Id { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public int ServicioId { get; set; }
    public string ServicioNombre { get; set; } = string.Empty;
    public int EmpleadoId { get; set; }
    public string EmpleadoNombre { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string Estado { get; set; } = string.Empty;

    // Solo Pendiente/Confirmada admiten editar o reagendar.
    public bool PuedeEditar { get; set; }

    // null = la cita todavía no tiene factura generada.
    public int? FacturaId { get; set; }
}
