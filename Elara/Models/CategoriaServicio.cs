namespace ElaraMVC.Models;

public class CategoriaServicio
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;

    public ICollection<Servicio> Servicios { get; set; } = new List<Servicio>();
}
