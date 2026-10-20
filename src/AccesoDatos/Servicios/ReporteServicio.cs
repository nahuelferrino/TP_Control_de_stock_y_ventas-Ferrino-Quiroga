using AccesoDatos.Reportes;
using AccesoDatos.Repositorios;

namespace AccesoDatos.Servicios;

public class ReporteServicio
{
    private readonly ReporteRepositorio reportes;

    public ReporteServicio(ReporteRepositorio reportes) => this.reportes = reportes;

    public List<StockBajoFila> StockBajo() => reportes.StockBajo();
    public List<InventarioCategoriaFila> InventarioCategoria() => reportes.InventarioCategoria();

    public List<HistorialClienteFila> HistorialCliente(int clienteId, DateTime desde, DateTime hasta)
    {
        if (clienteId <= 0) throw new ArgumentException("Seleccioná un cliente para consultar su historial.");
        var rango = ValidarFechas(desde, hasta);
        return reportes.HistorialCliente(clienteId, rango.Desde, rango.HastaExclusiva);
    }

    public List<VentaPeriodoFila> VentasPeriodo(DateTime desde, DateTime hasta)
    {
        var rango = ValidarFechas(desde, hasta);
        return reportes.VentasPeriodo(rango.Desde, rango.HastaExclusiva);
    }

    public List<ProductoMasVendidoFila> ProductosMasVendidos(DateTime desde, DateTime hasta)
    {
        var rango = ValidarFechas(desde, hasta);
        return reportes.ProductosMasVendidos(rango.Desde, rango.HastaExclusiva);
    }

    private static (DateTime Desde, DateTime HastaExclusiva) ValidarFechas(DateTime desde, DateTime hasta)
    {
        if (desde.Date > hasta.Date) throw new ArgumentException("La fecha desde no puede ser posterior a la fecha hasta.");
        if (hasta.Date == DateTime.MaxValue.Date) throw new ArgumentException("Seleccioná una fecha hasta anterior al 31/12/9999.");
        return (desde.Date, hasta.Date.AddDays(1));
    }
}
