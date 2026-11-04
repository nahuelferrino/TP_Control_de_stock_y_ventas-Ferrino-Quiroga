using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AccesoDatos.Contexto;

public static class ConfiguracionBaseDatos
{
    public static void Inicializar(IDbContextFactory<StockVentasDbContext> fabrica)
    {
        using var contexto = fabrica.CreateDbContext();
        contexto.Database.Migrate();
    }

    // La aplicación y las herramientas de migraciones usan la misma ruta.
    public static string RutaArchivo
    {
        get
        {
            var rutaConfigurada = Environment.GetEnvironmentVariable("STOCKVENTAS_DB_PATH");
            if (!string.IsNullOrWhiteSpace(rutaConfigurada))
                return Path.GetFullPath(rutaConfigurada);

            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "StockVentas", "stockventas.db");
        }
    }

    public static StockVentasDbContext CrearContexto()
    {
        var opciones = new DbContextOptionsBuilder<StockVentasDbContext>();
        Configurar(opciones);
        return new StockVentasDbContext(opciones.Options);
    }

    public static void Configurar(DbContextOptionsBuilder opciones)
    {
        var ruta = RutaArchivo;
        Directory.CreateDirectory(Path.GetDirectoryName(ruta)!);
        var conexion = new SqliteConnectionStringBuilder
        {
            DataSource = ruta,
            ForeignKeys = true
        };
        opciones.UseSqlite(conexion.ToString());
    }
}
