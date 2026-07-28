using System.ComponentModel.DataAnnotations;

namespace ElaraMVC.Models;

public class Cliente
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Telefono { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    public DateTime? FechaNacimiento { get; set; }

    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    [StringLength(500)]
    public string? Alergias { get; set; }

    [StringLength(500)]
    public string? Preferencias { get; set; }

    public string? FotoUrl { get; set; }

    public bool Activo { get; set; } = true;

    // Relación con el módulo de Citas (pendiente): historial de citas del cliente.
    // Cuando exista la entidad Cita con FK a ClienteId, agregar aquí:
    // public ICollection<Cita> Citas { get; set; } = new List<Cita>();
}
