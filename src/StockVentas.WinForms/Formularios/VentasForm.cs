using System.ComponentModel;
using AccesoDatos.Modelos;
using AccesoDatos.Servicios;
using AccesoDatos.Ventas;

namespace StockVentas.WinForms.Formularios;

public class VentasForm : Form
{
    private readonly ProductoServicio productos;
    private readonly ClienteServicio clientes;
    private readonly VentaServicio ventas;
    private readonly ComboBox cliente = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DateTimePicker fecha = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy HH:mm" };
    private readonly ComboBox producto = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown cantidad = new() { Minimum = 1, Maximum = int.MaxValue, Value = 1 };
    private readonly Label disponible = new() { AutoSize = true, Dock = DockStyle.Fill };
    private readonly Label total = new() { AutoSize = true, Font = new Font("Segoe UI", 15, FontStyle.Bold), ForeColor = ElementosInterfaz.Azul };
    private readonly Label estado = new() { AutoSize = true, Dock = DockStyle.Fill };
    private readonly DataGridView grilla = ElementosInterfaz.Grilla();
    private readonly BindingList<CarritoFila> carrito = new();
    private readonly Button guardar;
    private List<Producto> productosDisponibles = new();

    public VentasForm(ProductoServicio productos, ClienteServicio clientes, VentaServicio ventas)
    {
        this.productos = productos; this.clientes = clientes; this.ventas = ventas;
        Text = "Stock y Ventas — Registrar venta";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(1000, 650);
        Size = new Size(1140, 760);
        Font = new Font("Segoe UI", 10);
        BackColor = Color.FromArgb(245, 247, 250);
        AutoScaleMode = AutoScaleMode.Dpi;
        var raiz = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), ColumnCount = 1, RowCount = 6 };
        raiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        raiz.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        raiz.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        raiz.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        raiz.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        raiz.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        raiz.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        Controls.Add(raiz);
        raiz.Controls.Add(new Label { Text = "Registrar venta", Font = new Font(Font.FontFamily, 20, FontStyle.Bold), ForeColor = ElementosInterfaz.Azul, AutoSize = true }, 0, 0);
        var editor = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 4, RowCount = 3 };
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
        for (var i = 0; i < 3; i++) editor.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        ElementosInterfaz.Campo(editor, "Cliente *", cliente, 0, 0);
        ElementosInterfaz.Campo(editor, "Fecha *", fecha, 2, 0);
        ElementosInterfaz.Campo(editor, "Producto *", producto, 0, 1);
        ElementosInterfaz.Campo(editor, "Cantidad *", cantidad, 2, 1);
        editor.Controls.Add(disponible, 1, 2); editor.SetColumnSpan(disponible, 3);
        raiz.Controls.Add(editor, 0, 1);
        producto.SelectedIndexChanged += (_, _) => MostrarDisponibilidad();

        var acciones = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = true };
        acciones.Controls.Add(ElementosInterfaz.Boton("Agregar producto", (_, _) => ElementosInterfaz.Ejecutar(this, AgregarProducto), true));
        acciones.Controls.Add(ElementosInterfaz.Boton("Retirar seleccionado", (_, _) =>
        {
            if (grilla.SelectedRows.Count == 0) return;
            carrito.Remove((CarritoFila)grilla.SelectedRows[0].DataBoundItem);
        }));
        acciones.Controls.Add(ElementosInterfaz.Boton("Actualizar listas", (_, _) => ElementosInterfaz.Ejecutar(this, CargarListas)));
        raiz.Controls.Add(acciones, 0, 2);
        ElementosInterfaz.Columna(grilla, nameof(CarritoFila.Codigo), "Código", 80);
        ElementosInterfaz.Columna(grilla, nameof(CarritoFila.Nombre), "Producto", 180);
        ElementosInterfaz.Columna(grilla, nameof(CarritoFila.Cantidad), "Cantidad", 70);
        ElementosInterfaz.Columna(grilla, nameof(CarritoFila.PrecioUnitario), "Precio unitario", 100, "N2");
        ElementosInterfaz.Columna(grilla, nameof(CarritoFila.Subtotal), "Subtotal", 100, "N2");
        grilla.DataSource = carrito;
        raiz.Controls.Add(grilla, 0, 3);
        var pie = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = true };
        guardar = ElementosInterfaz.Boton("Confirmar venta", (_, _) => ElementosInterfaz.Ejecutar(this, GuardarVenta), true);
        pie.Controls.Add(guardar);
        pie.Controls.Add(ElementosInterfaz.Boton("Nueva venta", (_, _) =>
        {
            if (ConfirmarDescartar()) ElementosInterfaz.Ejecutar(this, NuevaVenta);
        }));
        total.Margin = new Padding(20, 9, 0, 0);
        pie.Controls.Add(total);
        raiz.Controls.Add(pie, 0, 4); raiz.Controls.Add(estado, 0, 5);
        carrito.ListChanged += (_, _) => ActualizarTotal();
        Load += (_, _) => ElementosInterfaz.Ejecutar(this, NuevaVenta);
        FormClosing += (_, evento) => evento.Cancel = !ConfirmarDescartar();
    }

    private bool ConfirmarDescartar() => carrito.Count == 0 || MessageBox.Show(this,
        "Hay productos en una venta sin guardar. ¿Descartar esa venta?", "Venta pendiente",
        MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes;

    private void CargarListas()
    {
        var clienteId = cliente.SelectedValue is int c ? c : 0;
        var productoId = producto.SelectedValue is int p ? p : 0;
        cliente.DisplayMember = "Nombre"; cliente.ValueMember = "Id";
        cliente.DataSource = clientes.Consultar().Select(c => new { c.Id, Nombre = c.Apellido + ", " + c.Nombre }).ToList();
        cliente.SelectedIndex = -1;
        if (clienteId > 0) cliente.SelectedValue = clienteId;
        productosDisponibles = productos.Consultar();
        producto.DisplayMember = "Nombre"; producto.ValueMember = "Id";
        producto.DataSource = productosDisponibles.Select(p => new { p.Id, Nombre = $"{p.Codigo} — {p.Nombre}" }).ToList();
        producto.SelectedIndex = -1;
        if (productoId > 0) producto.SelectedValue = productoId;
        MostrarDisponibilidad();
        estado.Text = cliente.Items.Count == 0 ? "Registrá primero un cliente desde el menú principal."
            : producto.Items.Count == 0 ? "Registrá primero productos desde el menú principal." : "Elegí productos y agregalos a la venta.";
    }

    private void MostrarDisponibilidad()
    {
        var seleccionado = producto.SelectedValue is int id ? productosDisponibles.FirstOrDefault(p => p.Id == id) : null;
        disponible.Text = seleccionado is null ? "Seleccioná un producto para consultar precio y stock."
            : $"Stock disponible: {seleccionado.StockDisponible} · Precio unitario: {seleccionado.Precio:N2}";
    }

    private void AgregarProducto()
    {
        var seleccionado = producto.SelectedValue is int id ? productosDisponibles.FirstOrDefault(p => p.Id == id) : null;
        if (seleccionado is null) throw new ArgumentException("Seleccioná un producto.");
        var existente = carrito.FirstOrDefault(f => f.ProductoId == seleccionado.Id);
        var unidades = (long)(existente?.Cantidad ?? 0) + (int)cantidad.Value;
        if (unidades > seleccionado.StockDisponible) throw new ArgumentException("La cantidad supera el stock disponible.");
        if (existente is null)
            carrito.Add(new CarritoFila { ProductoId = seleccionado.Id, Codigo = seleccionado.Codigo,
                Nombre = seleccionado.Nombre, Cantidad = (int)unidades, PrecioUnitario = seleccionado.Precio });
        else
        {
            existente.Cantidad = (int)unidades;
            carrito.ResetBindings();
        }
        cantidad.Value = 1;
    }

    private void ActualizarTotal()
    {
        total.Text = $"Total estimado: {carrito.Sum(f => f.Subtotal):N2}";
        guardar.Enabled = carrito.Count > 0;
    }

    private void NuevaVenta()
    {
        carrito.Clear();
        cliente.SelectedIndex = -1; producto.SelectedIndex = -1;
        cantidad.Value = 1; fecha.Value = DateTime.Now;
        CargarListas(); ActualizarTotal();
    }

    private void GuardarVenta()
    {
        if (cliente.SelectedValue is not int clienteId) throw new ArgumentException("Seleccioná un cliente.");
        if (MessageBox.Show(this, "¿Confirmar la venta y descontar los productos del stock?",
            "Confirmar venta", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
        var venta = ventas.Registrar(clienteId, fecha.Value,
            carrito.Select(f => new LineaVenta { ProductoId = f.ProductoId, Cantidad = f.Cantidad }).ToList());
        // Vaciar el carrito inmediatamente después de guardar evita confirmar dos veces.
        carrito.Clear();
        estado.Text = $"Venta #{venta.Id} guardada · Total: {venta.Total:N2}";
        MessageBox.Show(this, $"Venta #{venta.Id} guardada correctamente.\nTotal: {venta.Total:N2}", "Venta registrada", MessageBoxButtons.OK, MessageBoxIcon.Information);
        CargarListas();
        estado.Text = $"Venta #{venta.Id} guardada · Total: {venta.Total:N2}";
    }

    private sealed class CarritoFila
    {
        public int ProductoId { get; set; }
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal => Cantidad * PrecioUnitario;
    }
}
