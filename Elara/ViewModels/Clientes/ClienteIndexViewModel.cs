using ElaraMVC.Models;

namespace ElaraMVC.ViewModels;

public class ClienteIndexViewModel
{
    public List<Cliente> Clientes { get; set; } = new();
    public string SearchTerm { get; set; } = string.Empty;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}
