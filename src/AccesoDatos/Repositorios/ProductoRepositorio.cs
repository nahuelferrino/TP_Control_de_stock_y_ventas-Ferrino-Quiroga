using Microsoft.EntityFrameworkCore;
using AccesoDatos.Contexto;
using AccesoDatos.Modelos;

namespace AccesoDatos.Repositorios;

public class ProductoRepositorio
{
    private readonly Func<StockVentasDbContext> crearContexto;

    public ProductoRepositorio(Func<StockVentasDbContext> crearContexto)
        => this.crearContexto = crearContexto;

    public List<Producto> Consultar(string busqueda = "", bool incluirInactivos = false)
    {
        using var contexto = crearContexto();
        var consulta = contexto.Productos.AsNoTracking().Include(p => p.Proveedor).AsQueryable();
        if (!incluirInactivos)
            consulta = consulta.Where(p => p.Activo);

        var resultado = consulta.OrderBy(p => p.Nombre).ThenBy(p => p.Id).ToList();
        var texto = busqueda.Trim();
        if (texto.Length == 0) return resultado;

        // Para este prototipo, filtrar después de consultar SQLite permite comparar
        // también letras como í y ñ; upper() de SQLite solo transforma caracteres ASCII.
        return resultado.Where(p => p.Codigo.Contains(texto, StringComparison.OrdinalIgnoreCase)
            || p.Nombre.Contains(texto, StringComparison.OrdinalIgnoreCase)
            || p.Categoria.Contains(texto, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public Producto? ObtenerPorId(int id)
    {
        using var contexto = crearContexto();
        return contexto.Productos.AsNoTracking().Include(p => p.Proveedor).FirstOrDefault(p => p.Id == id);
    }

    public List<string> ConsultarCategorias()
    {
        using var contexto = crearContexto();
        // Incluir también las categorías de productos inactivos para conservarlas.
        return contexto.Productos.Select(p => p.Categoria).Distinct().ToList();
    }

    public bool ExisteCodigo(string codigo, int excluirId = 0)
    {
        using var contexto = crearContexto();
        return contexto.Productos.Any(p => p.Codigo == codigo && p.Id != excluirId);
    }

    public void Agregar(Producto producto)
    {
        using var contexto = crearContexto();
        contexto.Productos.Add(producto);
        contexto.SaveChanges();
    }

    public void Modificar(Producto producto)
    {
        using var contexto = crearContexto();
        var existente = contexto.Productos.Find(producto.Id)
            ?? throw new InvalidOperationException("El producto ya no existe.");
        if (!existente.Activo)
            throw new InvalidOperationException("No se puede modificar un producto dado de baja.");

        existente.Codigo = producto.Codigo;
        existente.Nombre = producto.Nombre;
        existente.Categoria = producto.Categoria;
        existente.Precio = producto.Precio;
        existente.StockDisponible = producto.StockDisponible;
        existente.StockMinimo = producto.StockMinimo;
        existente.ProveedorId = producto.ProveedorId;
        contexto.SaveChanges();
    }

    public void DarDeBaja(int id)
    {
        using var contexto = crearContexto();
        var producto = contexto.Productos.Find(id)
            ?? throw new InvalidOperationException("El producto ya no existe.");
        if (!producto.Activo)
            throw new InvalidOperationException("El producto ya está dado de baja.");

        // Baja lógica: se conserva el producto para no perder ventas anteriores.
        producto.Activo = false;
        contexto.SaveChanges();
    }
}
