namespace AccesoDatos.Modelos;

public class Proveedor
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public List<Producto> Productos { get; set; } = new();
}
