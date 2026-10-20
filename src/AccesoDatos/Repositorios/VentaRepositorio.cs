using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using AccesoDatos.Contexto;
using AccesoDatos.Modelos;
using AccesoDatos.Ventas;

namespace AccesoDatos.Repositorios;

public class VentaRepositorio
{
    private readonly Func<StockVentasDbContext> crearContexto;

    public VentaRepositorio(Func<StockVentasDbContext> crearContexto)
        => this.crearContexto = crearContexto;

    public Venta Registrar(int clienteId, DateTime fecha, IReadOnlyCollection<LineaVenta> lineas)
    {
        using var contexto = crearContexto();
        // El control de stock y el guardado forman una sola operación de escritura.
        using var transaccion = contexto.Database.BeginTransaction();
        var cliente = contexto.Clientes.Find(clienteId);
        if (cliente is null || !cliente.Activo)
            throw new ArgumentException("Seleccioná un cliente activo.");

        var ids = lineas.Select(l => l.ProductoId).ToList();
        var productos = contexto.Productos.Where(p => ids.Contains(p.Id)).ToDictionary(p => p.Id);
        var venta = new Venta { ClienteId = cliente.Id, Cliente = cliente, Fecha = fecha };
        foreach (var linea in lineas)
        {
            if (!productos.TryGetValue(linea.ProductoId, out var producto) || !producto.Activo)
                throw new ArgumentException("Uno de los productos ya no está disponible. Actualizá el listado.");
            if (linea.Cantidad <= 0 || producto.StockDisponible < linea.Cantidad)
                throw new ArgumentException($"Stock insuficiente para {producto.Nombre}. Disponible: {producto.StockDisponible}.");
            venta.Detalles.Add(new DetalleVenta
            {
                ProductoId = producto.Id,
                Cantidad = linea.Cantidad,
                PrecioUnitario = producto.Precio
            });
            producto.StockDisponible -= linea.Cantidad;
        }

        // La etapa no incluye login. Este registro identifica las ventas locales
        // y no tiene una contraseña de acceso conocida o compartida.
        var operador = contexto.Usuarios.SingleOrDefault(u => u.NombreUsuario == "operador-local");
        if (operador is null)
            operador = new Usuario
            {
                NombreUsuario = "operador-local",
                ClaveHash = Convert.ToHexString(SHA256.HashData(RandomNumberGenerator.GetBytes(32)))
            };
        if (!operador.Activo) throw new InvalidOperationException("El operador local está inactivo.");
        venta.Usuario = operador;
        contexto.Ventas.Add(venta);
        contexto.SaveChanges();
        transaccion.Commit();
        return venta;
    }
}
