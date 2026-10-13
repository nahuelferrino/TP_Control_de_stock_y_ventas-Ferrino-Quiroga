using System.ComponentModel.DataAnnotations.Schema;

namespace AccesoDatos.Modelos;

public class Venta
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public List<DetalleVenta> Detalles { get; set; } = new();
    public Calificacion? Calificacion { get; set; }

    // Para obtener el total al consultar una venta, cargar también sus detalles.
    [NotMapped]
    public decimal Total => Detalles.Sum(detalle => detalle.Subtotal);
}
