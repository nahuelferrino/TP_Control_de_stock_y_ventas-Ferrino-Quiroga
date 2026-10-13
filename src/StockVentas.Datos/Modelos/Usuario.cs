namespace StockVentas.Datos.Modelos;

public class Usuario
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string ClaveHash { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public List<Venta> Ventas { get; set; } = new();
}
