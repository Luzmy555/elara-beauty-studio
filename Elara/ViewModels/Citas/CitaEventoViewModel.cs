namespace ElaraMVC.ViewModels;

// Forma que espera FullCalendar para cada evento del feed JSON.
// System.Text.Json serializa las propiedades en camelCase por defecto en MVC.
public class CitaEventoViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string BackgroundColor { get; set; } = string.Empty;
    public string BorderColor { get; set; } = string.Empty;
    public string TextColor { get; set; } = "#FDFBF9";
    public bool Editable { get; set; }
    public CitaEventoExtendedProps ExtendedProps { get; set; } = new();
}

public class CitaEventoExtendedProps
{
    public string Estado { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public string Servicio { get; set; } = string.Empty;
    public string Empleado { get; set; } = string.Empty;

    // null = la cita todavía no tiene factura generada.
    public int? FacturaId { get; set; }
}
