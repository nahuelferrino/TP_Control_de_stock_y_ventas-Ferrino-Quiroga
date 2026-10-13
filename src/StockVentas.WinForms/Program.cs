using StockVentas.Datos.Contexto;
using StockVentas.Datos.Repositorios;
using StockVentas.Datos.Servicios;
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
            Application.Run(new ProductosForm(productoServicio, proveedorServicio));
        }
        catch (Exception error)
        {
            MessageBox.Show($"No se pudo iniciar la aplicación.\n\n{error.Message}\n\nBase de datos: {ConfiguracionBaseDatos.RutaArchivo}",
                "Stock y Ventas", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
