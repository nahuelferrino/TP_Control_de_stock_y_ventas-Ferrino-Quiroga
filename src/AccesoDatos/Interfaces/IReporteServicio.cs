using AccesoDatos.Modelos;
using AccesoDatos.Reportes;

namespace AccesoDatos.Interfaces;

public interface IReporteServicio
{
    List<StockBajoFila> StockBajo();
    List<InventarioCategoriaFila> InventarioCategoria();
    List<HistorialClienteFila> HistorialCliente(int clienteId, DateTime desde, DateTime hasta);
    List<VentaPeriodoFila> VentasPeriodo(DateTime desde, DateTime hasta);
    List<ProductoMasVendidoFila> ProductosMasVendidos(DateTime desde, DateTime hasta);
}
