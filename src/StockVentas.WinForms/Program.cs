using AccesoDatos.Contexto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StockVentas.WinForms.Formularios;

namespace StockVentas.WinForms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        try
        {
            var servicios = new ServiceCollection();
            servicios.AgregarAplicacion();
            using var contenedor = servicios.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });
            ConfiguracionBaseDatos.Inicializar(
                contenedor.GetRequiredService<IDbContextFactory<StockVentasDbContext>>());
            using var ambitoPrincipal = contenedor.CreateScope();
            Application.Run(ambitoPrincipal.ServiceProvider.GetRequiredService<PrincipalForm>());
        }
        catch (Exception error)
        {
            MessageBox.Show($"No se pudo iniciar la aplicación.\n\n{error.Message}\n\nBase de datos: {ConfiguracionBaseDatos.RutaArchivo}",
                "Stock y Ventas", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
