using AccesoDatos.Contexto;
using AccesoDatos.Repositorios;
using AccesoDatos.Servicios;
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
            ConfiguracionBaseDatos.Inicializar();
            var productos = new ProductoRepositorio(ConfiguracionBaseDatos.CrearContexto);
            var proveedores = new ProveedorRepositorio(ConfiguracionBaseDatos.CrearContexto);
            var productoServicio = new ProductoServicio(productos, proveedores);
            var proveedorServicio = new ProveedorServicio(proveedores);
            var clienteServicio = new ClienteServicio(new ClienteRepositorio(ConfiguracionBaseDatos.CrearContexto));
            var ventaServicio = new VentaServicio(new VentaRepositorio(ConfiguracionBaseDatos.CrearContexto));
            var reporteServicio = new ReporteServicio(new ReporteRepositorio(ConfiguracionBaseDatos.CrearContexto));
            Application.Run(new PrincipalForm(productoServicio, proveedorServicio, clienteServicio, ventaServicio, reporteServicio));
        }
        catch (Exception error)
        {
            MessageBox.Show($"No se pudo iniciar la aplicación.\n\n{error.Message}\n\nBase de datos: {ConfiguracionBaseDatos.RutaArchivo}",
                "Stock y Ventas", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
