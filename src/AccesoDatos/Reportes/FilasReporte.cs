namespace AccesoDatos.Reportes;

public class StockBajoFila
{
    public string Codigo { get; set; } = "";
    public string Producto { get; set; } = "";
    public string Categoria { get; set; } = "";
    public string Proveedor { get; set; } = "";
    public int StockDisponible { get; set; }
    public int StockMinimo { get; set; }
}

public class HistorialClienteFila
{
    public int VentaId { get; set; }
    public DateTime Fecha { get; set; }
    public string Producto { get; set; } = "";
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal => Cantidad * PrecioUnitario;
}

public class VentaPeriodoFila
{
    public int VentaId { get; set; }
    public DateTime Fecha { get; set; }
    public string Cliente { get; set; } = "";
    public long Unidades { get; set; }
    public decimal Total { get; set; }
}

public class ProductoMasVendidoFila
{
    public int ProductoId { get; set; }
    public string Codigo { get; set; } = "";
    public string Producto { get; set; } = "";
    public long Unidades { get; set; }
    public decimal Importe { get; set; }
}

public class InventarioCategoriaFila
{
    public string Categoria { get; set; } = "";
    public int Productos { get; set; }
    public long Unidades { get; set; }
    public decimal ValorStock { get; set; }
}
