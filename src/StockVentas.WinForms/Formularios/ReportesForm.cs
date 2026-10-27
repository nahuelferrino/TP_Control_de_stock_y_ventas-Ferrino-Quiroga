using AccesoDatos.Reportes;
using AccesoDatos.Servicios;

namespace StockVentas.WinForms.Formularios;

public class ReportesForm : Form
{
    private readonly ReporteServicio reportes;
    private readonly ClienteServicio clientes;
    private readonly ComboBox tipo = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox cliente = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DateTimePicker desde = new() { Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(-30) };
    private readonly DateTimePicker hasta = new() { Format = DateTimePickerFormat.Short, Value = DateTime.Today };
    private readonly DataGridView grilla = ElementosInterfaz.Grilla();
    private readonly Label descripcion = new() { Dock = DockStyle.Fill, AutoSize = true };
    private readonly Label estado = new() { Dock = DockStyle.Fill, AutoSize = true, ForeColor = ElementosInterfaz.Azul };

    public ReportesForm(ReporteServicio reportes, ClienteServicio clientes)
    {
        this.reportes = reportes; this.clientes = clientes;
        Text = "Stock y Ventas — Reportes";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(1050, 650);
        Size = new Size(1200, 760);
        Font = new Font("Segoe UI", 10);
        BackColor = Color.FromArgb(245, 247, 250);
        AutoScaleMode = AutoScaleMode.Dpi;
        var raiz = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), ColumnCount = 1, RowCount = 7 };
        raiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        raiz.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        raiz.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        raiz.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        raiz.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        raiz.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        raiz.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        raiz.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        Controls.Add(raiz);
        raiz.Controls.Add(new Label { Text = "Reportes y consultas", AutoSize = true, Font = new Font(Font.FontFamily, 20, FontStyle.Bold), ForeColor = ElementosInterfaz.Azul }, 0, 0);
        var selector = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 2, RowCount = 1 };
        selector.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        selector.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        selector.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        ElementosInterfaz.Campo(selector, "Reporte", tipo, 0, 0);
        raiz.Controls.Add(selector, 0, 1);
        tipo.DisplayMember = nameof(OpcionReporte.Nombre);
        tipo.Items.AddRange(new object[]
        {
            new OpcionReporte(0, "1. Productos con bajo stock"),
            new OpcionReporte(1, "2. Historial de ventas por cliente"),
            new OpcionReporte(2, "3. Ventas realizadas en un período"),
            new OpcionReporte(3, "4. Productos más vendidos"),
            new OpcionReporte(4, "5. Inventario por categoría")
        });
        var filtros = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 6, RowCount = 1 };
        filtros.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        filtros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        filtros.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        filtros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        filtros.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        filtros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        filtros.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        ElementosInterfaz.Campo(filtros, "Desde", desde, 0, 0);
        ElementosInterfaz.Campo(filtros, "Hasta", hasta, 2, 0);
        ElementosInterfaz.Campo(filtros, "Cliente", cliente, 4, 0);
        raiz.Controls.Add(filtros, 0, 2);
        raiz.Controls.Add(descripcion, 0, 3);
        var acciones = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = true };
        acciones.Controls.Add(ElementosInterfaz.Boton("Generar consulta", (_, _) => ElementosInterfaz.Ejecutar(this, Generar), true));
        raiz.Controls.Add(acciones, 0, 4);
        raiz.Controls.Add(grilla, 0, 5); raiz.Controls.Add(estado, 0, 6);
        tipo.SelectedIndexChanged += (_, _) => ActualizarFiltros();
        Load += (_, _) => ElementosInterfaz.Ejecutar(this, () =>
        {
            cliente.DisplayMember = "Nombre"; cliente.ValueMember = "Id";
            cliente.DataSource = clientes.Consultar(true).Select(c => new
            {
                c.Id, Nombre = $"{c.Apellido}, {c.Nombre}" + (c.Activo ? "" : " (inactivo)")
            }).ToList();
            cliente.SelectedIndex = -1;
            tipo.SelectedIndex = 0;
            Generar();
        });
    }

    private int Tipo => tipo.SelectedItem is OpcionReporte opcion ? opcion.Id : 0;

    private void ActualizarFiltros()
    {
        desde.Enabled = hasta.Enabled = Tipo is 1 or 2 or 3;
        cliente.Enabled = Tipo == 1;
        descripcion.Text = Tipo switch
        {
            0 => "Productos activos cuyo stock es menor o igual al mínimo. Consulta del inventario actual.",
            1 => "Elegí un cliente y un período. Se muestra cada producto comprado con su precio al momento de la venta.",
            2 => "Una fila por venta. El período incluye por completo los días desde y hasta.",
            3 => "Ranking por unidades vendidas en el período. Incluye productos que actualmente estén inactivos.",
            _ => "Productos activos agrupados por categoría. El valor del stock se calcula con el precio de venta actual."
        };
        grilla.DataSource = null; grilla.Columns.Clear();
        estado.Text = "Seleccioná los filtros habilitados y presioná Generar consulta.";
    }

    private void Generar()
    {
        grilla.DataSource = null; grilla.Columns.Clear();
        estado.Text = "Revisá los filtros si la consulta no se puede completar.";
        void Col(string propiedad, string titulo, int peso = 100, string? formato = null)
            => ElementosInterfaz.Columna(grilla, propiedad, titulo, peso, formato);
        switch (Tipo)
        {
            case 0:
            {
                var filas = reportes.StockBajo();
                Col(nameof(StockBajoFila.Codigo), "Código", 75);
                Col(nameof(StockBajoFila.Producto), "Producto", 160);
                Col(nameof(StockBajoFila.Categoria), "Categoría", 110);
                Col(nameof(StockBajoFila.Proveedor), "Proveedor", 130);
                Col(nameof(StockBajoFila.StockDisponible), "Stock", 60);
                Col(nameof(StockBajoFila.StockMinimo), "Mínimo", 60);
                Mostrar(filas, $"{filas.Count} producto(s) requieren atención.");
                break;
            }
            case 1:
            {
                var filas = reportes.HistorialCliente(cliente.SelectedValue is int id ? id : 0, desde.Value, hasta.Value);
                Col(nameof(HistorialClienteFila.VentaId), "Venta", 60);
                Col(nameof(HistorialClienteFila.Fecha), "Fecha", 110, "dd/MM/yyyy HH:mm");
                Col(nameof(HistorialClienteFila.Producto), "Producto", 180);
                Col(nameof(HistorialClienteFila.Cantidad), "Cantidad", 65);
                Col(nameof(HistorialClienteFila.PrecioUnitario), "Precio unitario", 100, "N2");
                Col(nameof(HistorialClienteFila.Subtotal), "Subtotal", 100, "N2");
                Mostrar(filas, $"{filas.Select(f => f.VentaId).Distinct().Count()} venta(s) · Total: {filas.Sum(f => f.Subtotal):N2}");
                break;
            }
            case 2:
            {
                var filas = reportes.VentasPeriodo(desde.Value, hasta.Value);
                Col(nameof(VentaPeriodoFila.VentaId), "Venta", 60);
                Col(nameof(VentaPeriodoFila.Fecha), "Fecha", 120, "dd/MM/yyyy HH:mm");
                Col(nameof(VentaPeriodoFila.Cliente), "Cliente", 190);
                Col(nameof(VentaPeriodoFila.Unidades), "Unidades", 75);
                Col(nameof(VentaPeriodoFila.Total), "Total", 110, "N2");
                Mostrar(filas, $"{filas.Count} venta(s) · Importe total: {filas.Sum(f => f.Total):N2}");
                break;
            }
            case 3:
            {
                var filas = reportes.ProductosMasVendidos(desde.Value, hasta.Value);
                Col(nameof(ProductoMasVendidoFila.Codigo), "Código", 85);
                Col(nameof(ProductoMasVendidoFila.Producto), "Producto", 190);
                Col(nameof(ProductoMasVendidoFila.Unidades), "Unidades vendidas", 100);
                Col(nameof(ProductoMasVendidoFila.Importe), "Importe vendido", 120, "N2");
                Mostrar(filas, $"{filas.Sum(f => f.Unidades)} unidad(es) · Importe total: {filas.Sum(f => f.Importe):N2}");
                break;
            }
            default:
            {
                var filas = reportes.InventarioCategoria();
                Col(nameof(InventarioCategoriaFila.Categoria), "Categoría", 170);
                Col(nameof(InventarioCategoriaFila.Productos), "Productos", 90);
                Col(nameof(InventarioCategoriaFila.Unidades), "Unidades en stock", 105);
                Col(nameof(InventarioCategoriaFila.ValorStock), "Valor a precio de venta", 150, "N2");
                Mostrar(filas, $"{filas.Sum(f => f.Productos)} producto(s) · Valor total del stock: {filas.Sum(f => f.ValorStock):N2}");
                break;
            }
        }
    }

    private void Mostrar<T>(List<T> filas, string resumen)
    {
        grilla.DataSource = filas;
        grilla.ClearSelection();
        estado.Text = filas.Count == 0 ? "No hay resultados para esta consulta." : resumen;
    }

    private sealed class OpcionReporte
    {
        public int Id { get; }
        public string Nombre { get; }
        public OpcionReporte(int id, string nombre) { Id = id; Nombre = nombre; }
    }
}
