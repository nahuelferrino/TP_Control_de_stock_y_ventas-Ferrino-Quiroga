using AccesoDatos.Modelos;
using AccesoDatos.Ventas;

namespace AccesoDatos.Interfaces;

public interface IVentaRepositorio
{
    Venta Registrar(int clienteId, DateTime fecha, IReadOnlyCollection<LineaVenta> lineas);
}
