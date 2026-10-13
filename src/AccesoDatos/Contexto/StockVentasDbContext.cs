using Microsoft.EntityFrameworkCore;
using AccesoDatos.Modelos;

namespace AccesoDatos.Contexto;

public class StockVentasDbContext : DbContext
{
    public StockVentasDbContext(DbContextOptions<StockVentasDbContext> options) : base(options) { }

    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();
    public DbSet<Calificacion> Calificaciones => Set<Calificacion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var producto = modelBuilder.Entity<Producto>();
        producto.ToTable("Productos", tabla =>
        {
            tabla.HasCheckConstraint("CK_Productos_Precio", "CAST(Precio AS NUMERIC) > 0");
            tabla.HasCheckConstraint("CK_Productos_StockDisponible", "StockDisponible >= 0");
            tabla.HasCheckConstraint("CK_Productos_StockMinimo", "StockMinimo >= 0");
        });
        producto.Property(p => p.Codigo).IsRequired().HasMaxLength(30);
        producto.HasIndex(p => p.Codigo).IsUnique();
        producto.Property(p => p.Nombre).IsRequired().HasMaxLength(100);
        producto.Property(p => p.Categoria).IsRequired().HasMaxLength(60);
        producto.Property(p => p.Precio).HasPrecision(18, 2);
        producto.HasOne(p => p.Proveedor).WithMany(p => p.Productos)
            .HasForeignKey(p => p.ProveedorId).OnDelete(DeleteBehavior.Restrict);

        var proveedor = modelBuilder.Entity<Proveedor>();
        proveedor.ToTable("Proveedores");
        proveedor.Property(p => p.Nombre).IsRequired().HasMaxLength(100);
        proveedor.Property(p => p.Telefono).HasMaxLength(30);
        proveedor.Property(p => p.Email).HasMaxLength(150);
        proveedor.Property(p => p.Direccion).HasMaxLength(200);

        var cliente = modelBuilder.Entity<Cliente>();
        cliente.ToTable("Clientes");
        cliente.Property(c => c.Nombre).IsRequired().HasMaxLength(100);
        cliente.Property(c => c.Apellido).IsRequired().HasMaxLength(100);
        cliente.Property(c => c.Telefono).HasMaxLength(30);
        cliente.Property(c => c.Email).HasMaxLength(150);

        var usuario = modelBuilder.Entity<Usuario>();
        usuario.ToTable("Usuarios");
        usuario.Property(u => u.NombreUsuario).IsRequired().HasMaxLength(50);
        usuario.HasIndex(u => u.NombreUsuario).IsUnique();
        usuario.Property(u => u.ClaveHash).IsRequired().HasMaxLength(256);

        var venta = modelBuilder.Entity<Venta>();
        venta.ToTable("Ventas");
        venta.HasOne(v => v.Cliente).WithMany(c => c.Ventas)
            .HasForeignKey(v => v.ClienteId).OnDelete(DeleteBehavior.Restrict);
        venta.HasOne(v => v.Usuario).WithMany(u => u.Ventas)
            .HasForeignKey(v => v.UsuarioId).OnDelete(DeleteBehavior.Restrict);

        var detalle = modelBuilder.Entity<DetalleVenta>();
        detalle.ToTable("DetallesVenta", tabla =>
        {
            tabla.HasCheckConstraint("CK_DetallesVenta_Cantidad", "Cantidad > 0");
            tabla.HasCheckConstraint("CK_DetallesVenta_Precio", "CAST(PrecioUnitario AS NUMERIC) > 0");
        });
        detalle.Property(d => d.PrecioUnitario).HasPrecision(18, 2);
        detalle.HasOne(d => d.Venta).WithMany(v => v.Detalles)
            .HasForeignKey(d => d.VentaId).OnDelete(DeleteBehavior.Cascade);
        detalle.HasOne(d => d.Producto).WithMany(p => p.DetallesVenta)
            .HasForeignKey(d => d.ProductoId).OnDelete(DeleteBehavior.Restrict);

        var calificacion = modelBuilder.Entity<Calificacion>();
        calificacion.ToTable("Calificaciones", tabla =>
            tabla.HasCheckConstraint("CK_Calificaciones_Puntuacion", "Puntuacion BETWEEN 1 AND 5"));
        calificacion.Property(c => c.Comentario).HasMaxLength(500);
        calificacion.HasOne(c => c.Venta).WithOne(v => v.Calificacion)
            .HasForeignKey<Calificacion>(c => c.VentaId).OnDelete(DeleteBehavior.Cascade);
    }
}
