using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AccesoDatos.Contexto;
using AccesoDatos.Interfaces;
using AccesoDatos.Repositorios;
using AccesoDatos.Servicios;

namespace AccesoDatos.Configuracion;

public static class ConfiguracionServicios
{
    public static IServiceCollection AgregarAccesoDatos(this IServiceCollection servicios,
        Action<DbContextOptionsBuilder>? configurarContexto = null)
    {
        // La fábrica es compartida, pero cada operación crea y libera su propio contexto.
        servicios.AddDbContextFactory<StockVentasDbContext>(
            configurarContexto ?? ConfiguracionBaseDatos.Configurar);

        servicios.AddScoped<IProductoRepositorio, ProductoRepositorio>();
        servicios.AddScoped<IProveedorRepositorio, ProveedorRepositorio>();
        servicios.AddScoped<IClienteRepositorio, ClienteRepositorio>();
        servicios.AddScoped<IVentaRepositorio, VentaRepositorio>();
        servicios.AddScoped<IReporteRepositorio, ReporteRepositorio>();

        servicios.AddScoped<IProductoServicio, ProductoServicio>();
        servicios.AddScoped<IProveedorServicio, ProveedorServicio>();
        servicios.AddScoped<IClienteServicio, ClienteServicio>();
        servicios.AddScoped<IVentaServicio, VentaServicio>();
        servicios.AddScoped<IReporteServicio, ReporteServicio>();
        return servicios;
    }
}
