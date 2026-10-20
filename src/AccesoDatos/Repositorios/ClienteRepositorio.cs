using Microsoft.EntityFrameworkCore;
using AccesoDatos.Contexto;
using AccesoDatos.Modelos;

namespace AccesoDatos.Repositorios;

public class ClienteRepositorio
{
    private readonly Func<StockVentasDbContext> crearContexto;

    public ClienteRepositorio(Func<StockVentasDbContext> crearContexto)
        => this.crearContexto = crearContexto;

    public List<Cliente> Consultar(bool incluirInactivos = false)
    {
        using var contexto = crearContexto();
        var consulta = contexto.Clientes.AsNoTracking().AsQueryable();
        if (!incluirInactivos) consulta = consulta.Where(c => c.Activo);
        return consulta.OrderBy(c => c.Apellido).ThenBy(c => c.Nombre).ThenBy(c => c.Id).ToList();
    }

    public void Agregar(Cliente cliente)
    {
        using var contexto = crearContexto();
        contexto.Clientes.Add(cliente);
        contexto.SaveChanges();
    }

    public void Modificar(Cliente cliente)
    {
        using var contexto = crearContexto();
        var existente = contexto.Clientes.Find(cliente.Id)
            ?? throw new InvalidOperationException("El cliente ya no existe.");
        if (!existente.Activo)
            throw new InvalidOperationException("No se puede modificar un cliente dado de baja.");
        existente.Nombre = cliente.Nombre;
        existente.Apellido = cliente.Apellido;
        existente.Telefono = cliente.Telefono;
        existente.Email = cliente.Email;
        contexto.SaveChanges();
    }

    public void DarDeBaja(int id)
    {
        using var contexto = crearContexto();
        var cliente = contexto.Clientes.Find(id)
            ?? throw new InvalidOperationException("El cliente ya no existe.");
        if (!cliente.Activo) throw new InvalidOperationException("El cliente ya está dado de baja.");
        cliente.Activo = false;
        contexto.SaveChanges();
    }
}
