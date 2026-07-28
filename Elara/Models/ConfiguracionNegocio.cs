namespace ElaraMVC.Models;

// Fila única (singleton) con los datos generales del negocio. La crea
// ConfiguracionNegocioService la primera vez que se pide y a partir de ahí
// siempre se edita la misma fila.
public class ConfiguracionNegocio
{
    public int Id { get; set; }

    public string NombreSalon { get; set; } = "Elara";
    public string? LogoUrl { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? EmailContacto { get; set; }

    public Moneda Moneda { get; set; } = Moneda.DOP;

    public ICollection<HorarioNegocio> Horarios { get; set; } = new List<HorarioNegocio>();
}
