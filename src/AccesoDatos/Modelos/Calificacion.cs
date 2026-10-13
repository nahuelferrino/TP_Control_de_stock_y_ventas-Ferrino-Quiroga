namespace AccesoDatos.Modelos;

public class Calificacion
{
    public int Id { get; set; }
    public int VentaId { get; set; }
    public Venta Venta { get; set; } = null!;
    public int Puntuacion { get; set; }
    public string? Comentario { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
}
