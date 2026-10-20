using Microsoft.EntityFrameworkCore;
using AccesoDatos.Contexto;
using AccesoDatos.Modelos;
using AccesoDatos.Repositorios;
using AccesoDatos.Servicios;
using AccesoDatos.Ventas;

namespace StockVentas.Pruebas;

internal static partial class Program
{
    private static void EjecutarPruebasEtapa3(string carpeta)
    {
        var opciones = new DbContextOptionsBuilder<StockVentasDbContext>()
            .UseSqlite($"Data Source={Path.Combine(carpeta, "etapa3.db")};Foreign Keys=True").Options;
        StockVentasDbContext CrearContexto() => new(opciones);
        var clientes = new ClienteServicio(new ClienteRepositorio(CrearContexto));
        var proveedores = new ProveedorServicio(new ProveedorRepositorio(CrearContexto));
        var productos = new ProductoServicio(new ProductoRepositorio(CrearContexto), new ProveedorRepositorio(CrearContexto));
        var ventas = new VentaServicio(new VentaRepositorio(CrearContexto));
        var reportes = new ReporteServicio(new ReporteRepositorio(CrearContexto));
        var ana = new Cliente { Nombre = " Ana ", Apellido = " Pérez ", Email = "ana@example.com" };
        var bruno = new Cliente { Nombre = "Bruno", Apellido = "Quiroga" };
        var proveedor = new Proveedor { Nombre = "Distribuidora" };
        var a = new Producto { Codigo = "A-1", Nombre = "Cuaderno", Categoria = "Librería", Precio = 10.25m, StockDisponible = 10, StockMinimo = 2 };
        var b = new Producto { Codigo = "B-1", Nombre = "Lapicera", Categoria = "librería", Precio = 2.50m, StockDisponible = 8, StockMinimo = 5 };
        var c = new Producto { Codigo = "C-1", Nombre = "Cable", Categoria = "Electrónica", Precio = 15m, StockDisponible = 10, StockMinimo = 1 };
        var inactivo = new Producto { Codigo = "I-1", Nombre = "Discontinuado", Categoria = "Librería", Precio = 5m, StockDisponible = 100, StockMinimo = 100 };
        var dia = DateTime.Today.AddDays(-7);
        LineaVenta Linea(Producto p, int cantidad) => new() { ProductoId = p.Id, Cantidad = cantidad };
        int Stock(Producto p)
        {
            using var contexto = CrearContexto();
            return contexto.Productos.Single(x => x.Id == p.Id).StockDisponible;
        }
        int CantidadVentas()
        {
            using var contexto = CrearContexto();
            return contexto.Ventas.Count();
        }

        Probar("Etapa 3: preparar clientes y productos en una base independiente", () =>
        {
            using var contexto = CrearContexto();
            contexto.Database.Migrate();
            Verificar(!contexto.Database.HasPendingModelChanges());
            clientes.Guardar(ana); clientes.Guardar(bruno); proveedores.Guardar(proveedor);
            foreach (var p in new[] { a, b, c, inactivo })
            {
                p.ProveedorId = proveedor.Id;
                productos.Guardar(p);
            }
            productos.DarDeBaja(inactivo.Id);
            Verificar(clientes.Consultar().Count == 2 && ana.Nombre == "Ana" && ana.Apellido == "Pérez");
        });
        Probar("Clientes: rechazo de campos obligatorios y email inválido", () =>
        {
            DebeFallar<ArgumentException>(() => clientes.Guardar(new Cliente { Nombre = "Ana" }));
            DebeFallar<ArgumentException>(() => clientes.Guardar(new Cliente { Apellido = "Pérez" }));
            DebeFallar<ArgumentException>(() => clientes.Guardar(new Cliente { Nombre = "Ana", Apellido = "Pérez", Email = "incorrecto" }));
            Verificar(clientes.Consultar().Count == 2);
        });
        Probar("Clientes: la modificación persiste al volver a consultar", () =>
        {
            ana.Telefono = "123456";
            clientes.Guardar(ana);
            Verificar(clientes.Consultar().Single(x => x.Id == ana.Id).Telefono == "123456");
        });
        Probar("Los reportes de ventas vacíos devuelven listas vacías", () =>
        {
            Verificar(reportes.HistorialCliente(ana.Id, dia, dia).Count == 0);
            Verificar(reportes.VentasPeriodo(dia, dia).Count == 0);
            Verificar(reportes.ProductosMasVendidos(dia, dia).Count == 0);
        });
        Probar("Una venta fallida no crea ni operador, ni venta, ni descuento de stock", () =>
        {
            DebeFallar<ArgumentException>(() => ventas.Registrar(ana.Id, dia, new[] { Linea(a, 1), Linea(b, 999) }));
            using var contexto = CrearContexto();
            Verificar(contexto.Ventas.Count() == 0 && contexto.Usuarios.Count() == 0 && Stock(a) == 10 && Stock(b) == 8);
        });
        Probar("Ventas: rechazo de cliente, carrito vacío y fecha futura", () =>
        {
            DebeFallar<ArgumentException>(() => ventas.Registrar(0, dia, new[] { Linea(a, 1) }));
            DebeFallar<ArgumentException>(() => ventas.Registrar(99999, dia, new[] { Linea(a, 1) }));
            DebeFallar<ArgumentException>(() => ventas.Registrar(ana.Id, dia, Array.Empty<LineaVenta>()));
            DebeFallar<ArgumentException>(() => ventas.Registrar(ana.Id, DateTime.Today.AddDays(1), new[] { Linea(a, 1) }));
            Verificar(CantidadVentas() == 0);
        });
        Probar("Ventas: rechazo de cantidades cero o negativas", () =>
        {
            DebeFallar<ArgumentException>(() => ventas.Registrar(ana.Id, dia, new[] { Linea(a, 0) }));
            DebeFallar<ArgumentException>(() => ventas.Registrar(ana.Id, dia, new[] { Linea(a, -1) }));
            Verificar(Stock(a) == 10);
        });
        Probar("Ventas: rechazo de productos inexistentes o inactivos", () =>
        {
            DebeFallar<ArgumentException>(() => ventas.Registrar(ana.Id, dia, new[] { new LineaVenta { ProductoId = 99999, Cantidad = 1 } }));
            DebeFallar<ArgumentException>(() => ventas.Registrar(ana.Id, dia, new[] { Linea(inactivo, 1) }));
            Verificar(CantidadVentas() == 0);
        });
        Probar("Venta real con varios productos: total, detalles y stock persistentes", () =>
        {
            var venta = ventas.Registrar(ana.Id, dia.AddHours(8), new[] { Linea(a, 2), Linea(b, 1) });
            Verificar(venta.Id > 0 && venta.Total == 23m && venta.Detalles.Count == 2 && venta.UsuarioId > 0);
            Verificar(Stock(a) == 8 && Stock(b) == 7);
            using var contexto = CrearContexto();
            Verificar(contexto.Ventas.Include(v => v.Detalles).Single().Total == 23m);
        });
        Probar("Ventas al final del día y al comienzo del día siguiente", () =>
        {
            ventas.Registrar(bruno.Id, dia.AddDays(1).AddTicks(-1), new[] { Linea(a, 1), Linea(c, 2) });
            ventas.Registrar(ana.Id, dia.AddDays(1), new[] { Linea(b, 2) });
            Verificar(CantidadVentas() == 3 && Stock(a) == 7 && Stock(b) == 5 && Stock(c) == 8);
        });
        Probar("Las ventas reutilizan el mismo operador local", () =>
        {
            using var contexto = CrearContexto();
            Verificar(contexto.Usuarios.Count() == 1 && contexto.Usuarios.Single().NombreUsuario == "operador-local");
        });
        Probar("Reporte 1: stock bajo incluye igualdad al mínimo y excluye productos inactivos", () =>
        {
            var filas = reportes.StockBajo();
            Verificar(filas.Count == 1 && filas[0].Codigo == "B-1" && filas[0].StockDisponible == 5);
        });
        Probar("Reporte 2: historial por cliente separa compras y conserva precios unitarios", () =>
        {
            var filas = reportes.HistorialCliente(ana.Id, dia, dia);
            Verificar(filas.Count == 2 && filas.Select(f => f.VentaId).Distinct().Count() == 1);
            Verificar(filas.Sum(f => f.Subtotal) == 23m && filas.Single(f => f.Producto == "Cuaderno").PrecioUnitario == 10.25m);
            Verificar(reportes.HistorialCliente(ana.Id, dia, dia.AddDays(1)).Sum(f => f.Subtotal) == 28m);
        });
        Probar("Reporte 3: período incluye todo el último día, sin incluir el siguiente", () =>
        {
            var filas = reportes.VentasPeriodo(dia.AddHours(12), dia.AddHours(1));
            Verificar(filas.Count == 2 && filas.Sum(f => f.Total) == 63.25m && filas.Sum(f => f.Unidades) == 6);
            Verificar(reportes.VentasPeriodo(dia.AddDays(1), dia.AddDays(1)).Single().Total == 5m);
        });
        Probar("Reporte 4: ranking por unidades suma productos de ventas diferentes", () =>
        {
            var filas = reportes.ProductosMasVendidos(dia, dia);
            Verificar(filas.Count == 3 && filas[0].Codigo == "A-1" && filas[0].Unidades == 3 && filas[0].Importe == 30.75m);
            Verificar(filas.Sum(f => f.Importe) == 63.25m);
        });
        Probar("Reporte 4: los empates se ordenan por código y el período filtra las unidades", () =>
        {
            var filas = reportes.ProductosMasVendidos(dia, dia.AddDays(1));
            Verificar(filas[0].Codigo == "A-1" && filas[0].Unidades == 3 && filas[1].Codigo == "B-1" && filas[1].Unidades == 3);
        });
        Probar("Reporte 5: categorías sin duplicados por mayúsculas, unidades y valoración exactas", () =>
        {
            var filas = reportes.InventarioCategoria();
            var libreria = filas.Single(f => f.Categoria.Equals("Librería", StringComparison.OrdinalIgnoreCase));
            Verificar(filas.Count == 2 && libreria.Productos == 2 && libreria.Unidades == 12 && libreria.ValorStock == 84.25m);
            Verificar(filas.Sum(f => f.ValorStock) == 204.25m && filas.Sum(f => f.Productos) == 3);
        });
        Probar("Reportes: rechazo de fechas invertidas y de cliente sin seleccionar", () =>
        {
            DebeFallar<ArgumentException>(() => reportes.VentasPeriodo(dia.AddDays(1), dia));
            DebeFallar<ArgumentException>(() => reportes.ProductosMasVendidos(dia.AddDays(1), dia));
            DebeFallar<ArgumentException>(() => reportes.HistorialCliente(ana.Id, dia.AddDays(1), dia));
            DebeFallar<ArgumentException>(() => reportes.HistorialCliente(0, dia, dia));
            DebeFallar<ArgumentException>(() => reportes.HistorialCliente(99999, dia, dia));
        });
        Probar("Reportes: un período sin movimientos no muestra ventas de otros días", () =>
        {
            Verificar(reportes.VentasPeriodo(dia.AddDays(3), dia.AddDays(4)).Count == 0);
            Verificar(reportes.ProductosMasVendidos(dia.AddDays(3), dia.AddDays(4)).Count == 0);
            Verificar(reportes.HistorialCliente(ana.Id, dia.AddDays(3), dia.AddDays(4)).Count == 0);
        });
        Probar("Modificar el precio actual no cambia los importes de los reportes históricos", () =>
        {
            a.Precio = 99m; a.StockDisponible = Stock(a);
            productos.Guardar(a);
            Verificar(reportes.VentasPeriodo(dia, dia).Sum(f => f.Total) == 63.25m);
            Verificar(reportes.ProductosMasVendidos(dia, dia).Single(f => f.Codigo == a.Codigo).Importe == 30.75m);
            Verificar(reportes.HistorialCliente(ana.Id, dia, dia).Sum(f => f.Subtotal) == 23m);
        });
        Probar("Una venta rechazada no descuenta parcialmente ni crea detalles", () =>
        {
            var cantidadVentas = CantidadVentas();
            DebeFallar<ArgumentException>(() => ventas.Registrar(ana.Id, dia, new[] { Linea(a, 1), Linea(b, 999) }));
            Verificar(CantidadVentas() == cantidadVentas && Stock(a) == 7 && Stock(b) == 5);
            using var contexto = CrearContexto();
            Verificar(contexto.DetallesVenta.Count() == 5);
        });
        Probar("Los renglones repetidos se agrupan y descuentan una sola cantidad consolidada", () =>
        {
            var venta = ventas.Registrar(ana.Id, dia, new[] { Linea(c, 1), Linea(c, 1) });
            Verificar(venta.Detalles.Count == 1 && venta.Detalles.Single().Cantidad == 2 && Stock(c) == 6);
        });
        Probar("Se puede vender el stock exacto y se rechaza una venta posterior", () =>
        {
            ventas.Registrar(bruno.Id, dia, new[] { Linea(b, 5) });
            var cantidadVentas = CantidadVentas();
            DebeFallar<ArgumentException>(() => ventas.Registrar(ana.Id, dia, new[] { Linea(b, 1) }));
            Verificar(Stock(b) == 0 && CantidadVentas() == cantidadVentas);
            Verificar(reportes.StockBajo().Single(f => f.Codigo == b.Codigo).StockDisponible == 0);
        });
        Probar("Dos ventas simultáneas no pueden consumir la misma última unidad", () =>
        {
            b.StockDisponible = 1;
            productos.Guardar(b);
            var cantidadVentas = CantidadVentas();
            bool IntentarVenta()
            {
                try { ventas.Registrar(ana.Id, dia, new[] { Linea(b, 1) }); return true; }
                catch (ArgumentException) { return false; }
            }
            var primera = Task.Run(IntentarVenta);
            var segunda = Task.Run(IntentarVenta);
            Task.WaitAll(primera, segunda);
            Verificar((primera.Result ? 1 : 0) + (segunda.Result ? 1 : 0) == 1);
            Verificar(Stock(b) == 0 && CantidadVentas() == cantidadVentas + 1);
        });
        Probar("Dar de baja un producto conserva su presencia en el ranking histórico", () =>
        {
            productos.DarDeBaja(a.Id);
            Verificar(reportes.ProductosMasVendidos(dia, dia).Single(f => f.ProductoId == a.Id).Unidades == 3);
            Verificar(reportes.InventarioCategoria().Sum(f => f.Productos) == 2);
        });
        Probar("Dar de baja un cliente conserva su historial y bloquea nuevas ventas", () =>
        {
            clientes.DarDeBaja(ana.Id);
            Verificar(clientes.Consultar().Count == 1 && clientes.Consultar(true).Count == 2);
            Verificar(reportes.HistorialCliente(ana.Id, dia, dia).Count > 0);
            DebeFallar<ArgumentException>(() => ventas.Registrar(ana.Id, dia, new[] { Linea(c, 1) }));
            DebeFallar<InvalidOperationException>(() => clientes.Guardar(ana));
        });
    }
}
