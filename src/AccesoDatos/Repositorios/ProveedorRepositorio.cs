using AccesoDatos.Interfaces;
using Microsoft.EntityFrameworkCore;
using AccesoDatos.Contexto;
using AccesoDatos.Modelos;

namespace AccesoDatos.Repositorios;

public class ProveedorRepositorio : IProveedorRepositorio
{
    private readonly IDbContextFactory<StockVentasDbContext> fabricaContextos;

    public ProveedorRepositorio(IDbContextFactory<StockVentasDbContext> fabricaContextos)
        => this.fabricaContextos = fabricaContextos;

    public List<Proveedor> Consultar(bool incluirInactivos = false)
    {
        using var contexto = fabricaContextos.CreateDbContext();
        var consulta = contexto.Proveedores.AsNoTracking().AsQueryable();
        if (!incluirInactivos)
            consulta = consulta.Where(p => p.Activo);
        return consulta.OrderBy(p => p.Nombre).ThenBy(p => p.Id).ToList();
    }

    public Proveedor? ObtenerPorId(int id)
    {
        using var contexto = fabricaContextos.CreateDbContext();
        return contexto.Proveedores.AsNoTracking().FirstOrDefault(p => p.Id == id);
    }

    public void Agregar(Proveedor proveedor)
    {
        using var contexto = fabricaContextos.CreateDbContext();
        contexto.Proveedores.Add(proveedor);
        contexto.SaveChanges();
    }

    public void Modificar(Proveedor proveedor)
    {
        using var contexto = fabricaContextos.CreateDbContext();
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
        using var contexto = fabricaContextos.CreateDbContext();
        var proveedor = contexto.Proveedores.Find(id)
            ?? throw new InvalidOperationException("El proveedor ya no existe.");
        if (!proveedor.Activo)
            throw new InvalidOperationException("El proveedor ya está dado de baja.");
        proveedor.Activo = false;
        contexto.SaveChanges();
    }
}
