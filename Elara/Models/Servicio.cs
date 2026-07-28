namespace ElaraMVC.Models;

public class Servicio
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public int CategoriaServicioId { get; set; }
    public CategoriaServicio? CategoriaServicio { get; set; }

    public string Descripcion { get; set; } = string.Empty;

    // En minutos: define el bloque de tiempo que reservará el módulo de Citas
    // al calcular disponibilidad de horario.
    public int DuracionMinutos { get; set; }

    public decimal Precio { get; set; }
    public string? ImagenUrl { get; set; }
    public bool Activo { get; set; } = true;

    // Relación con el módulo de Citas (pendiente): citas que incluyeron este servicio.
    // public ICollection<Cita> Citas { get; set; } = new List<Cita>();
}
