using Microsoft.Extensions.DependencyInjection;
using AccesoDatos.Configuracion;
using StockVentas.WinForms.Formularios;

namespace StockVentas.WinForms;

public static class ConfiguracionAplicacion
{
    public static IServiceCollection AgregarAplicacion(this IServiceCollection servicios)
    {
        servicios.AgregarAccesoDatos();
        servicios.AddSingleton<NavegacionFormularios>();
        servicios.AddTransient<PrincipalForm>();
        servicios.AddTransient<ProductosForm>();
        servicios.AddTransient<ProveedoresForm>();
        servicios.AddTransient<ClientesForm>();
        servicios.AddTransient<VentasForm>();
        servicios.AddTransient<ReportesForm>();
        return servicios;
    }
}
