using Microsoft.Extensions.DependencyInjection;

namespace StockVentas.WinForms;

public sealed class NavegacionFormularios
{
    private readonly IServiceScopeFactory fabricaAmbitos;

    public NavegacionFormularios(IServiceScopeFactory fabricaAmbitos)
        => this.fabricaAmbitos = fabricaAmbitos;

    public void Abrir<TFormulario>(IWin32Window propietario) where TFormulario : Form
    {
        // El ámbito pertenece a esta apertura, incluso al abrir una ventana desde otra.
        using var ambito = fabricaAmbitos.CreateScope();
        var formulario = ambito.ServiceProvider.GetRequiredService<TFormulario>();
        formulario.ShowDialog(propietario);
        // Al terminar ShowDialog, el ámbito libera el formulario y sus dependencias.
    }
}
