using AccesoDatos.Modelos;
using AccesoDatos.Repositorios;
using AccesoDatos.Ventas;

namespace AccesoDatos.Servicios;

public class VentaServicio
{
    private readonly VentaRepositorio ventas;

    public VentaServicio(VentaRepositorio ventas) => this.ventas = ventas;

    public Venta Registrar(int clienteId, DateTime fecha, IReadOnlyCollection<LineaVenta> lineas)
    {
        if (clienteId <= 0) throw new ArgumentException("Seleccioná un cliente.");
        if (fecha.Date > DateTime.Today) throw new ArgumentException("La fecha de venta no puede ser futura.");
        if (lineas is null || lineas.Count == 0) throw new ArgumentException("Agregá al menos un producto a la venta.");
        if (lineas.Any(l => l is null || l.ProductoId <= 0 || l.Cantidad <= 0))
            throw new ArgumentException("Cada producto debe tener una cantidad entera mayor que cero.");

        var agrupadas = new List<LineaVenta>();
        foreach (var grupo in lineas.GroupBy(l => l.ProductoId))
        {
            var cantidad = grupo.Sum(l => (long)l.Cantidad);
            if (cantidad > int.MaxValue) throw new ArgumentException("La cantidad solicitada es demasiado grande.");
            agrupadas.Add(new LineaVenta { ProductoId = grupo.Key, Cantidad = (int)cantidad });
        }
        return ventas.Registrar(clienteId, fecha, agrupadas);
    }
}
