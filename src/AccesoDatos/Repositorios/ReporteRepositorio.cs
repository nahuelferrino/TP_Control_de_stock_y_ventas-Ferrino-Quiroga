using Microsoft.EntityFrameworkCore;
using AccesoDatos.Contexto;
using AccesoDatos.Reportes;

namespace AccesoDatos.Repositorios;

public class ReporteRepositorio
{
    private readonly Func<StockVentasDbContext> crearContexto;

    public ReporteRepositorio(Func<StockVentasDbContext> crearContexto)
        => this.crearContexto = crearContexto;

    public List<StockBajoFila> StockBajo()
    {
        using var contexto = crearContexto();
        return contexto.Productos.AsNoTracking()
            .Where(p => p.Activo && p.StockDisponible <= p.StockMinimo)
            .OrderBy(p => p.StockDisponible).ThenBy(p => p.Nombre)
            .Select(p => new StockBajoFila
            {
                Codigo = p.Codigo, Producto = p.Nombre, Categoria = p.Categoria,
                Proveedor = p.Proveedor.Nombre, StockDisponible = p.StockDisponible, StockMinimo = p.StockMinimo
            }).ToList();
    }

    public List<HistorialClienteFila> HistorialCliente(int clienteId, DateTime desde, DateTime hastaExclusiva)
    {
        using var contexto = crearContexto();
        if (!contexto.Clientes.Any(c => c.Id == clienteId))
            throw new ArgumentException("El cliente seleccionado ya no existe.");
        return contexto.DetallesVenta.AsNoTracking()
            .Where(d => d.Venta.ClienteId == clienteId && d.Venta.Fecha >= desde && d.Venta.Fecha < hastaExclusiva)
            .OrderByDescending(d => d.Venta.Fecha).ThenBy(d => d.VentaId).ThenBy(d => d.Id)
            .Select(d => new HistorialClienteFila
            {
                VentaId = d.VentaId, Fecha = d.Venta.Fecha, Producto = d.Producto.Nombre,
                Cantidad = d.Cantidad, PrecioUnitario = d.PrecioUnitario
            }).ToList();
    }

    public List<VentaPeriodoFila> VentasPeriodo(DateTime desde, DateTime hastaExclusiva)
    {
        using var contexto = crearContexto();
        var ventas = contexto.Ventas.AsNoTracking().Include(v => v.Cliente).Include(v => v.Detalles)
            .Where(v => v.Fecha >= desde && v.Fecha < hastaExclusiva)
            .OrderByDescending(v => v.Fecha).ThenByDescending(v => v.Id).ToList();
        // Los importes decimales se suman en C# para conservar su precisión con SQLite.
        return ventas.Select(v => new VentaPeriodoFila
        {
            VentaId = v.Id, Fecha = v.Fecha, Cliente = v.Cliente.Apellido + ", " + v.Cliente.Nombre,
            Unidades = v.Detalles.Sum(d => (long)d.Cantidad), Total = v.Total
        }).ToList();
    }

    public List<ProductoMasVendidoFila> ProductosMasVendidos(DateTime desde, DateTime hastaExclusiva)
    {
        using var contexto = crearContexto();
        var detalles = contexto.DetallesVenta.AsNoTracking()
            .Where(d => d.Venta.Fecha >= desde && d.Venta.Fecha < hastaExclusiva)
            .Select(d => new
            {
                d.ProductoId, d.Producto.Codigo, d.Producto.Nombre, d.Cantidad, d.PrecioUnitario
            }).ToList();
        return detalles.GroupBy(d => d.ProductoId).Select(grupo => new ProductoMasVendidoFila
        {
            ProductoId = grupo.Key, Codigo = grupo.First().Codigo, Producto = grupo.First().Nombre,
            Unidades = grupo.Sum(d => (long)d.Cantidad), Importe = grupo.Sum(d => d.Cantidad * d.PrecioUnitario)
        }).OrderByDescending(fila => fila.Unidades).ThenBy(fila => fila.Codigo).ToList();
    }

    public List<InventarioCategoriaFila> InventarioCategoria()
    {
        using var contexto = crearContexto();
        var productos = contexto.Productos.AsNoTracking().Where(p => p.Activo)
            .Select(p => new { p.Categoria, p.StockDisponible, p.Precio }).ToList();
        return productos.GroupBy(p => p.Categoria.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(grupo => new InventarioCategoriaFila
            {
                Categoria = grupo.Key, Productos = grupo.Count(),
                Unidades = grupo.Sum(p => (long)p.StockDisponible),
                ValorStock = grupo.Sum(p => p.StockDisponible * p.Precio)
            }).OrderBy(fila => fila.Categoria).ToList();
    }
}
