namespace ElaraMVC.Models;

public class Producto
{
    // Umbral de "cerca del mínimo": 30% por encima de la cantidad mínima.
    // Constante de negocio simple, fácil de ajustar si el salón lo pide.
    private const decimal FactorCercaDelMinimo = 1.3m;

    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public CategoriaProducto Categoria { get; set; }
    public string Marca { get; set; } = string.Empty;

    public decimal CantidadActual { get; set; }
    public decimal CantidadMinima { get; set; }
    public UnidadMedida UnidadMedida { get; set; }

    public decimal PrecioCosto { get; set; }
    public string? Proveedor { get; set; }
    public DateTime? FechaUltimaCompra { get; set; }

    public ICollection<MovimientoInventario> Movimientos { get; set; } = new List<MovimientoInventario>();

    // Método estático (nunca lo mapea EF Core) reutilizado por el repositorio,
    // el servicio y las vistas para clasificar el nivel de stock de forma
    // consistente en un solo lugar.
    public static NivelStock CalcularNivelStock(decimal cantidadActual, decimal cantidadMinima)
    {
        if (cantidadActual < cantidadMinima)
        {
            return NivelStock.Bajo;
        }

        if (cantidadActual <= cantidadMinima * FactorCercaDelMinimo)
        {
            return NivelStock.Cerca;
        }

        return NivelStock.Bien;
    }
}
