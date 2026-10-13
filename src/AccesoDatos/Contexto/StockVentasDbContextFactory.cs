using Microsoft.EntityFrameworkCore.Design;

namespace AccesoDatos.Contexto;

// Permite ejecutar dotnet ef sin iniciar WinForms, también desde macOS.
public class StockVentasDbContextFactory : IDesignTimeDbContextFactory<StockVentasDbContext>
{
    public StockVentasDbContext CreateDbContext(string[] args)
        => ConfiguracionBaseDatos.CrearContexto();
}
