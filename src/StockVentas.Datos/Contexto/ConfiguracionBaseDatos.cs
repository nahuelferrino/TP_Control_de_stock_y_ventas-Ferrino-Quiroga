using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace StockVentas.Datos.Contexto;

public static class ConfiguracionBaseDatos
{
    public static void Inicializar()
    {
        using var contexto = CrearContexto();
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
        Directory.CreateDirectory(Path.GetDirectoryName(RutaArchivo)!);
        var conexion = new SqliteConnectionStringBuilder
        {
            DataSource = RutaArchivo,
            ForeignKeys = true
        };
        var opciones = new DbContextOptionsBuilder<StockVentasDbContext>()
            .UseSqlite(conexion.ToString()).Options;
        return new StockVentasDbContext(opciones);
    }
}
