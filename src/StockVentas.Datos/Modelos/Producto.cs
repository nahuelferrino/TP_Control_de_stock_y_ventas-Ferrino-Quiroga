namespace StockVentas.Datos.Modelos;

public class Producto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int StockDisponible { get; set; }
    public int StockMinimo { get; set; }
    public bool Activo { get; set; } = true;
    public int ProveedorId { get; set; }
    public Proveedor Proveedor { get; set; } = null!;
    public List<DetalleVenta> DetallesVenta { get; set; } = new();
}
