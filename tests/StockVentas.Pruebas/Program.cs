using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using AccesoDatos.Contexto;
using AccesoDatos.Modelos;
using AccesoDatos.Repositorios;
using AccesoDatos.Servicios;

namespace StockVentas.Pruebas;

// Pruebas de integración: utilizan un archivo SQLite temporal, sin tocar la base del usuario.
internal static class Program
{
    private static int correctas;
    private static int fallidas;

    private static int Main()
    {
        var carpeta = Path.Combine(Path.GetTempPath(), "StockVentasPruebas-" + Guid.NewGuid());
        Directory.CreateDirectory(carpeta);
        var ruta = Path.Combine(carpeta, "pruebas.db");
        var opciones = new DbContextOptionsBuilder<StockVentasDbContext>()
            .UseSqlite(new SqliteConnectionStringBuilder { DataSource = ruta, ForeignKeys = true }.ToString()).Options;
        StockVentasDbContext CrearContexto() => new(opciones);

        var proveedorRepositorio = new ProveedorRepositorio(CrearContexto);
        var productoRepositorio = new ProductoRepositorio(CrearContexto);
        var proveedores = new ProveedorServicio(proveedorRepositorio);
        var productos = new ProductoServicio(productoRepositorio, proveedorRepositorio);
        var proveedor = new Proveedor { Nombre = "  Distribuidora de prueba  ", Email = "contacto@example.com" };
        var producto = new Producto
        {
            Codigo = "  cu-001  ", Nombre = "Cuaderno", Categoria = "Librería",
            Precio = 12.50m, StockDisponible = 10, StockMinimo = 3
        };
        var ventaId = 0;

        try
        {
            Probar("La migración crea una base real y sus siete tablas", () =>
            {
                using var contexto = CrearContexto();
                contexto.Database.Migrate();
                Verificar(contexto.Database.GetAppliedMigrations().Single().EndsWith("InitialCreate"));
                Verificar(File.Exists(ruta) && new FileInfo(ruta).Length > 0);
                Verificar(contexto.Productos.Count() == 0 && contexto.Proveedores.Count() == 0
                    && contexto.Clientes.Count() == 0 && contexto.Usuarios.Count() == 0
                    && contexto.Ventas.Count() == 0 && contexto.DetallesVenta.Count() == 0
                    && contexto.Calificaciones.Count() == 0);
            });
            Probar("Aplicar nuevamente la migración no duplica el esquema", () =>
            {
                using var contexto = CrearContexto();
                contexto.Database.Migrate();
                Verificar(contexto.Database.GetAppliedMigrations().Count() == 1);
            });
            Probar("Alta de proveedor y persistencia en un contexto nuevo", () =>
            {
                proveedores.Guardar(proveedor);
                using var contexto = CrearContexto();
                Verificar(proveedor.Id > 0 && contexto.Proveedores.Single().Nombre == "Distribuidora de prueba");
            });
            Probar("Rechazo de proveedor sin nombre", () =>
                DebeFallar<ArgumentException>(() => proveedores.Guardar(new Proveedor())));
            Probar("Rechazo de email inválido", () =>
                DebeFallar<ArgumentException>(() => proveedores.Guardar(new Proveedor { Nombre = "Otro", Email = "no-es-email" })));
            Probar("Modificación persistente del proveedor", () =>
            {
                proveedor.Telefono = "123456";
                proveedores.Guardar(proveedor);
                Verificar(proveedores.Consultar().Single().Telefono == "123456");
            });
            Probar("Alta de producto: código normalizado y proveedor asociado", () =>
            {
                producto.ProveedorId = proveedor.Id;
                productos.Guardar(producto);
                using var contexto = CrearContexto();
                var guardado = contexto.Productos.Include(p => p.Proveedor).Single();
                Verificar(producto.Id > 0 && guardado.Codigo == "CU-001" && guardado.Proveedor.Id == proveedor.Id);
            });
            Probar("Consulta por nombre, categoría y código sin distinguir mayúsculas", () =>
            {
                Verificar(productos.Consultar("cuaderno").Count == 1);
                Verificar(productos.Consultar("librería").Count == 1);
                Verificar(productos.Consultar("cu-001").Count == 1);
                Verificar(productos.Consultar("inexistente").Count == 0);
            });
            Producto NuevoProducto() => new()
            {
                Codigo = "OTRO", Nombre = "Otro", Categoria = "Librería",
                Precio = 10m, StockDisponible = 1, StockMinimo = 0, ProveedorId = proveedor.Id
            };
            Probar("Rechazo de código duplicado con distinta capitalización", () =>
            {
                var otro = NuevoProducto(); otro.Codigo = " cu-001 ";
                DebeFallar<ArgumentException>(() => productos.Guardar(otro));
            });
            Probar("Rechazo de stock negativo", () =>
            {
                var otro = NuevoProducto(); otro.StockDisponible = -1;
                DebeFallar<ArgumentException>(() => productos.Guardar(otro));
            });
            Probar("Rechazo de stock mínimo negativo", () =>
            {
                var otro = NuevoProducto(); otro.StockMinimo = -1;
                DebeFallar<ArgumentException>(() => productos.Guardar(otro));
            });
            Probar("Rechazo de precio cero o con más de dos decimales", () =>
            {
                var otro = NuevoProducto(); otro.Precio = 0;
                DebeFallar<ArgumentException>(() => productos.Guardar(otro));
                otro.Precio = 1.234m;
                DebeFallar<ArgumentException>(() => productos.Guardar(otro));
            });
            Probar("Rechazo de producto sin proveedor válido", () =>
            {
                var otro = NuevoProducto(); otro.ProveedorId = 99999;
                DebeFallar<ArgumentException>(() => productos.Guardar(otro));
            });
            Probar("Modificación del producto sin rechazar su propio código", () =>
            {
                producto.Nombre = "Cuaderno actualizado";
                producto.StockDisponible = 8;
                productos.Guardar(producto);
                using var contexto = CrearContexto();
                var guardado = contexto.Productos.Single();
                Verificar(guardado.Nombre == "Cuaderno actualizado" && guardado.StockDisponible == 8);
            });
            Probar("SQLite también impide códigos duplicados", () =>
            {
                using var contexto = CrearContexto();
                var otro = NuevoProducto(); otro.Codigo = producto.Codigo;
                contexto.Productos.Add(otro);
                DebeFallar<DbUpdateException>(() => contexto.SaveChanges());
            });
            Probar("SQLite también impide stock negativo", () =>
            {
                using var contexto = CrearContexto();
                var otro = NuevoProducto(); otro.StockDisponible = -1;
                contexto.Productos.Add(otro);
                DebeFallar<DbUpdateException>(() => contexto.SaveChanges());
            });
            Probar("SQLite impide productos con proveedores inexistentes", () =>
            {
                using var contexto = CrearContexto();
                var otro = NuevoProducto(); otro.ProveedorId = 99999;
                contexto.Productos.Add(otro);
                DebeFallar<DbUpdateException>(() => contexto.SaveChanges());
            });
            Probar("No se puede eliminar físicamente un proveedor con productos", () =>
            {
                using var contexto = CrearContexto();
                contexto.Proveedores.Remove(contexto.Proveedores.Single());
                DebeFallar<DbUpdateException>(() => contexto.SaveChanges());
            });
            Probar("Persistencia de las relaciones Venta, Cliente, Usuario y DetalleVenta", () =>
            {
                using var contexto = CrearContexto();
                var venta = new Venta
                {
                    Cliente = new Cliente { Nombre = "Cliente", Apellido = "Prueba" },
                    Usuario = new Usuario { NombreUsuario = "pruebas", ClaveHash = "hash-solo-para-la-prueba" },
                    Detalles = new List<DetalleVenta>
                    {
                        new() { ProductoId = producto.Id, Cantidad = 2, PrecioUnitario = 12.50m }
                    }
                };
                contexto.Ventas.Add(venta);
                contexto.SaveChanges();
                ventaId = venta.Id;
                Verificar(venta.Total == 25m);
            });
            Probar("Cambiar el precio actual conserva el precio histórico de la venta", () =>
            {
                producto.Precio = 99m;
                productos.Guardar(producto);
                using var contexto = CrearContexto();
                var venta = contexto.Ventas.Include(v => v.Detalles).Single();
                Verificar(venta.Total == 25m && venta.Detalles.Single().PrecioUnitario == 12.50m);
            });
            Probar("Una venta admite una calificación con comentario opcional", () =>
            {
                using var contexto = CrearContexto();
                contexto.Calificaciones.Add(new Calificacion { VentaId = ventaId, Puntuacion = 5 });
                contexto.SaveChanges();
                Verificar(contexto.Calificaciones.Single().Comentario is null);
            });
            Probar("SQLite rechaza puntuaciones fuera de 1 a 5", () =>
            {
                foreach (var puntuacion in new[] { 0, 6 })
                {
                    using var contexto = CrearContexto();
                    contexto.Calificaciones.Single().Puntuacion = puntuacion;
                    DebeFallar<DbUpdateException>(() => contexto.SaveChanges());
                }
            });
            Probar("Una venta no admite dos calificaciones", () =>
            {
                using var contexto = CrearContexto();
                contexto.Calificaciones.Add(new Calificacion { VentaId = ventaId, Puntuacion = 4 });
                DebeFallar<DbUpdateException>(() => contexto.SaveChanges());
            });
            Probar("La baja lógica del producto conserva la venta y su detalle", () =>
            {
                productos.DarDeBaja(producto.Id);
                using var contexto = CrearContexto();
                var venta = contexto.Ventas.Include(v => v.Detalles).ThenInclude(d => d.Producto).Single();
                Verificar(!venta.Detalles.Single().Producto.Activo && venta.Total == 25m);
                Verificar(productos.Consultar().Count == 0 && productos.Consultar(incluirInactivos: true).Count == 1);
            });
            Probar("No se puede modificar un producto dado de baja", () =>
                DebeFallar<InvalidOperationException>(() => productos.Guardar(producto)));
            Probar("El código de un producto inactivo sigue reservado", () =>
            {
                var otro = NuevoProducto(); otro.Codigo = producto.Codigo;
                DebeFallar<ArgumentException>(() => productos.Guardar(otro));
            });
            Probar("La baja lógica del proveedor conserva sus productos", () =>
            {
                proveedores.DarDeBaja(proveedor.Id);
                Verificar(proveedores.Consultar().Count == 0 && proveedores.Consultar(true).Count == 1);
                using var contexto = CrearContexto();
                Verificar(contexto.Productos.Single().ProveedorId == proveedor.Id);
            });
            Probar("No se pueden guardar productos con proveedor inactivo", () =>
                DebeFallar<ArgumentException>(() => productos.Guardar(NuevoProducto())));
            Probar("No se puede modificar un proveedor dado de baja", () =>
                DebeFallar<InvalidOperationException>(() => proveedores.Guardar(proveedor)));
            Probar("Un nuevo inicio mantiene registros y el historial de migraciones", () =>
            {
                using var contexto = CrearContexto();
                contexto.Database.Migrate();
                Verificar(contexto.Productos.Count() == 1 && contexto.Proveedores.Count() == 1
                    && contexto.Ventas.Count() == 1 && contexto.Calificaciones.Count() == 1);
                Verificar(contexto.Database.GetAppliedMigrations().Count() == 1);
            });
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            Directory.Delete(carpeta, recursive: true);
        }
        Console.WriteLine($"\nResultado: {correctas} pruebas correctas; {fallidas} fallidas.");
        return fallidas == 0 ? 0 : 1;
    }

    private static void Probar(string nombre, Action prueba)
    {
        try { prueba(); correctas++; Console.WriteLine($"OK — {nombre}"); }
        catch (Exception error) { fallidas++; Console.WriteLine($"ERROR — {nombre}: {error}"); }
    }

    private static void Verificar(bool condicion)
    {
        if (!condicion) throw new Exception("El resultado no coincide con el esperado.");
    }

    private static void DebeFallar<T>(Action accion) where T : Exception
    {
        try { accion(); }
        catch (T) { return; }
        throw new Exception($"Se esperaba una excepción {typeof(T).Name}.");
    }
}
