using Microsoft.EntityFrameworkCore;
using StockVentas.Datos.Contexto;
using StockVentas.Datos.Modelos;

namespace StockVentas.Datos.Repositorios;

public class ProveedorRepositorio
{
    private readonly Func<StockVentasDbContext> crearContexto;

    public ProveedorRepositorio(Func<StockVentasDbContext> crearContexto)
        => this.crearContexto = crearContexto;

    public List<Proveedor> Consultar(bool incluirInactivos = false)
    {
        using var contexto = crearContexto();
        var consulta = contexto.Proveedores.AsNoTracking().AsQueryable();
        if (!incluirInactivos)
            consulta = consulta.Where(p => p.Activo);
        return consulta.OrderBy(p => p.Nombre).ThenBy(p => p.Id).ToList();
    }

    public Proveedor? ObtenerPorId(int id)
    {
        using var contexto = crearContexto();
        return contexto.Proveedores.AsNoTracking().FirstOrDefault(p => p.Id == id);
    }

    public void Agregar(Proveedor proveedor)
    {
        using var contexto = crearContexto();
        contexto.Proveedores.Add(proveedor);
        contexto.SaveChanges();
    }

    public void Modificar(Proveedor proveedor)
    {
        using var contexto = crearContexto();
        var existente = contexto.Proveedores.Find(proveedor.Id)
            ?? throw new InvalidOperationException("El proveedor ya no existe.");
        if (!existente.Activo)
            throw new InvalidOperationException("No se puede modificar un proveedor dado de baja.");
        existente.Nombre = proveedor.Nombre;
        existente.Telefono = proveedor.Telefono;
        existente.Email = proveedor.Email;
        existente.Direccion = proveedor.Direccion;
        contexto.SaveChanges();
    }

    public void DarDeBaja(int id)
    {
        using var contexto = crearContexto();
        var proveedor = contexto.Proveedores.Find(id)
            ?? throw new InvalidOperationException("El proveedor ya no existe.");
        if (!proveedor.Activo)
            throw new InvalidOperationException("El proveedor ya está dado de baja.");
        proveedor.Activo = false;
        contexto.SaveChanges();
    }
}
