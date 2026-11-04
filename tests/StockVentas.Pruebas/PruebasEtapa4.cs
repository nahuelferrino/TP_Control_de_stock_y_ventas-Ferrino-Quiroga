using AccesoDatos.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AccesoDatos.Configuracion;
using AccesoDatos.Contexto;
using AccesoDatos.Modelos;
using AccesoDatos.Repositorios;
using AccesoDatos.Servicios;

namespace StockVentas.Pruebas;

internal static partial class Program
{
    private static void EjecutarPruebasEtapa4(string carpeta)
    {
        var registros = new ServiceCollection();
        registros.AgregarAccesoDatos(opciones => opciones
            .UseSqlite($"Data Source={Path.Combine(carpeta, "etapa4.db")};Foreign Keys=True"));
        using var contenedor = registros.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true, ValidateScopes = true
        });

        Probar("DI: el contenedor resuelve los cinco servicios con todas sus dependencias", () =>
        {
            using var ambito = contenedor.CreateScope();
            var servicios = ambito.ServiceProvider;
            Verificar(servicios.GetRequiredService<IProductoServicio>() is not null);
            Verificar(servicios.GetRequiredService<IProveedorServicio>() is not null);
            Verificar(servicios.GetRequiredService<IClienteServicio>() is not null);
            Verificar(servicios.GetRequiredService<IVentaServicio>() is not null);
            Verificar(servicios.GetRequiredService<IReporteServicio>() is not null);
        });
        Probar("DI: cada contrato se resuelve con su implementación registrada", () =>
        {
            using var ambito = contenedor.CreateScope();
            var servicios = ambito.ServiceProvider;
            Verificar(servicios.GetRequiredService<IProductoServicio>() is ProductoServicio);
            Verificar(servicios.GetRequiredService<IProveedorServicio>() is ProveedorServicio);
            Verificar(servicios.GetRequiredService<IClienteServicio>() is ClienteServicio);
            Verificar(servicios.GetRequiredService<IVentaServicio>() is VentaServicio);
            Verificar(servicios.GetRequiredService<IReporteServicio>() is ReporteServicio);
            Verificar(servicios.GetRequiredService<IProductoRepositorio>() is ProductoRepositorio);
            Verificar(servicios.GetRequiredService<IProveedorRepositorio>() is ProveedorRepositorio);
            Verificar(servicios.GetRequiredService<IClienteRepositorio>() is ClienteRepositorio);
            Verificar(servicios.GetRequiredService<IVentaRepositorio>() is VentaRepositorio);
            Verificar(servicios.GetRequiredService<IReporteRepositorio>() is ReporteRepositorio);
        });
        Probar("DI: se puede sustituir un repositorio sin modificar el servicio", () =>
        {
            var configuracion = new ServiceCollection();
            configuracion.AgregarAccesoDatos();
            configuracion.AddScoped<IProveedorRepositorio, ProveedorRepositorioAlternativo>();
            using var proveedor = configuracion.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateOnBuild = true, ValidateScopes = true
            });
            using var ambito = proveedor.CreateScope();
            var servicio = ambito.ServiceProvider.GetRequiredService<IProveedorServicio>();
            Verificar(servicio is ProveedorServicio);
            Verificar(servicio.Consultar().Single().Nombre == "Proveedor alternativo de prueba");
        });
        Probar("DI: un mismo ámbito comparte servicios y repositorios", () =>
        {
            using var ambito = contenedor.CreateScope();
            Verificar(ReferenceEquals(ambito.ServiceProvider.GetRequiredService<IProductoServicio>(),
                ambito.ServiceProvider.GetRequiredService<IProductoServicio>()));
            Verificar(ReferenceEquals(ambito.ServiceProvider.GetRequiredService<IProductoRepositorio>(),
                ambito.ServiceProvider.GetRequiredService<IProductoRepositorio>()));
        });
        Probar("DI: ámbitos diferentes reciben servicios y repositorios diferentes", () =>
        {
            using var primero = contenedor.CreateScope();
            using var segundo = contenedor.CreateScope();
            Verificar(!ReferenceEquals(primero.ServiceProvider.GetRequiredService<IProductoServicio>(),
                segundo.ServiceProvider.GetRequiredService<IProductoServicio>()));
            Verificar(!ReferenceEquals(primero.ServiceProvider.GetRequiredService<IProductoRepositorio>(),
                segundo.ServiceProvider.GetRequiredService<IProductoRepositorio>()));
        });
        Probar("DI: la fábrica compartida produce contextos y seguimientos independientes", () =>
        {
            using var primero = contenedor.CreateScope();
            using var segundo = contenedor.CreateScope();
            var fabrica = primero.ServiceProvider.GetRequiredService<IDbContextFactory<StockVentasDbContext>>();
            Verificar(ReferenceEquals(fabrica, segundo.ServiceProvider.GetRequiredService<IDbContextFactory<StockVentasDbContext>>()));
            using var uno = fabrica.CreateDbContext();
            using var dos = fabrica.CreateDbContext();
            Verificar(!ReferenceEquals(uno, dos));
            uno.Proveedores.Add(new Proveedor { Nombre = "Sin guardar" });
            Verificar(uno.ChangeTracker.Entries().Count() == 1 && !dos.ChangeTracker.Entries().Any());
        });
        Probar("DI: se detecta un servicio scoped solicitado fuera de un ámbito", () =>
            DebeFallar<InvalidOperationException>(() => contenedor.GetRequiredService<IProductoServicio>()));
        Probar("DI: una dependencia faltante se detecta al construir el contenedor", () =>
        {
            var incompletos = new ServiceCollection();
            incompletos.AddScoped<IProductoServicio, ProductoServicio>();
            DebeFallar<AggregateException>(() =>
            {
                using var invalido = incompletos.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });
            });
        });
        Probar("DI: cerrar el ámbito libera los contextos resueltos por el contenedor", () =>
        {
            var ambito = contenedor.CreateScope();
            var contexto = ambito.ServiceProvider.GetRequiredService<StockVentasDbContext>();
            ambito.Dispose();
            DebeFallar<ObjectDisposedException>(() => contexto.Productos.Any());
        });
        Probar("DI: migraciones y persistencia funcionan entre ámbitos diferentes", () =>
        {
            ConfiguracionBaseDatos.Inicializar(contenedor.GetRequiredService<IDbContextFactory<StockVentasDbContext>>());
            using (var primero = contenedor.CreateScope())
                primero.ServiceProvider.GetRequiredService<IProveedorServicio>().Guardar(new Proveedor { Nombre = "Proveedor DI" });
            using var segundo = contenedor.CreateScope();
            Verificar(segundo.ServiceProvider.GetRequiredService<IProveedorServicio>().Consultar().Single().Nombre == "Proveedor DI");
            using var contexto = contenedor.GetRequiredService<IDbContextFactory<StockVentasDbContext>>().CreateDbContext();
            Verificar(!contexto.Database.HasPendingModelChanges());
        });
        Probar("DI: la configuración de ejecución y la de migraciones usan la misma ruta", () =>
        {
            var anterior = Environment.GetEnvironmentVariable("STOCKVENTAS_DB_PATH");
            try
            {
                var ruta = Path.Combine(carpeta, "configuracion.db");
                Environment.SetEnvironmentVariable("STOCKVENTAS_DB_PATH", ruta);
                var configuracion = new ServiceCollection();
                configuracion.AgregarAccesoDatos();
                using var proveedor = configuracion.BuildServiceProvider();
                using var ejecucion = proveedor.GetRequiredService<IDbContextFactory<StockVentasDbContext>>().CreateDbContext();
                using var migraciones = new StockVentasDbContextFactory().CreateDbContext(Array.Empty<string>());
                Verificar(ejecucion.Database.GetDbConnection().DataSource == ruta);
                Verificar(migraciones.Database.GetDbConnection().DataSource == ruta);
            }
            finally { Environment.SetEnvironmentVariable("STOCKVENTAS_DB_PATH", anterior); }
        });
    }

    private sealed class ProveedorRepositorioAlternativo : IProveedorRepositorio
    {
        public List<Proveedor> Consultar(bool incluirInactivos = false)
            => new() { new Proveedor { Id = 1, Nombre = "Proveedor alternativo de prueba" } };
        public Proveedor? ObtenerPorId(int id) => Consultar().FirstOrDefault(p => p.Id == id);
        public void Agregar(Proveedor proveedor) => throw new NotSupportedException();
        public void Modificar(Proveedor proveedor) => throw new NotSupportedException();
        public void DarDeBaja(int id) => throw new NotSupportedException();
    }
}
