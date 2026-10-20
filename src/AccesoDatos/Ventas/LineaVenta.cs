namespace AccesoDatos.Ventas;

// Datos enviados por el formulario. El precio se obtiene de la base al guardar.
public class LineaVenta
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
}
