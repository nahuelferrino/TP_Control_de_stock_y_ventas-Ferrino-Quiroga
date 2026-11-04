using AccesoDatos.Modelos;
using AccesoDatos.Reportes;

namespace AccesoDatos.Interfaces;

public interface IReporteRepositorio
{
    List<StockBajoFila> StockBajo();
    List<HistorialClienteFila> HistorialCliente(int clienteId, DateTime desde, DateTime hastaExclusiva);
    List<VentaPeriodoFila> VentasPeriodo(DateTime desde, DateTime hastaExclusiva);
    List<ProductoMasVendidoFila> ProductosMasVendidos(DateTime desde, DateTime hastaExclusiva);
    List<InventarioCategoriaFila> InventarioCategoria();
}
