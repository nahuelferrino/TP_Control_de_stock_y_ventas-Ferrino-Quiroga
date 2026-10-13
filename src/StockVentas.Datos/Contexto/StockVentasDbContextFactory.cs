using Microsoft.EntityFrameworkCore.Design;

namespace StockVentas.Datos.Contexto;

// Permite ejecutar dotnet ef sin iniciar WinForms, también desde macOS.
public class StockVentasDbContextFactory : IDesignTimeDbContextFactory<StockVentasDbContext>
{
    public StockVentasDbContext CreateDbContext(string[] args)
        => ConfiguracionBaseDatos.CrearContexto();
}
