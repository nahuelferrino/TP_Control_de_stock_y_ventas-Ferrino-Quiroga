using AccesoDatos.Modelos;
using AccesoDatos.Servicios;

namespace StockVentas.WinForms.Formularios;

public class ProductosForm : Form
{
    private readonly ProductoServicio productos;
    private readonly ProveedorServicio proveedores;
    private readonly TextBox codigo = new() { MaxLength = 30 };
    private readonly TextBox nombre = new() { MaxLength = 100 };
    private readonly TextBox categoria = new() { MaxLength = 60 };
    private readonly ComboBox proveedor = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown precio = new() { DecimalPlaces = 2, Maximum = 999999999.99m, ThousandsSeparator = true };
    private readonly NumericUpDown stock = new() { Maximum = int.MaxValue, ThousandsSeparator = true };
    private readonly NumericUpDown stockMinimo = new() { Maximum = int.MaxValue, ThousandsSeparator = true };
    private readonly TextBox busqueda = new() { Width = 310, PlaceholderText = "Código, nombre o categoría" };
    private readonly CheckBox incluirInactivos = new() { Text = "Mostrar inactivos", AutoSize = true, Margin = new Padding(12, 9, 8, 0) };
    private readonly DataGridView grilla = ElementosInterfaz.Grilla();
    private readonly Label modo = new() { Text = "Nuevo producto", AutoSize = true, Anchor = AnchorStyles.Left };
    private readonly Label estado = new() { AutoSize = true, Dock = DockStyle.Fill, ForeColor = ElementosInterfaz.Azul };
    private readonly Button guardar;
    private readonly Button baja;
    private int productoId;
    private bool actualizandoGrilla;

    public ProductosForm(ProductoServicio productos, ProveedorServicio proveedores)
    {
        this.productos = productos;
        this.proveedores = proveedores;
        Text = "Stock y Ventas — Productos";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1000, 670);
        Size = new Size(1170, 790);
        Font = new Font("Segoe UI", 10);
        BackColor = Color.FromArgb(245, 247, 250);
        AutoScaleMode = AutoScaleMode.Dpi;

        var raiz = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), ColumnCount = 1, RowCount = 6 };
        raiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
        raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 170));
        raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        raiz.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        Controls.Add(raiz);

        var cabecera = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        cabecera.Controls.Add(new Label { Text = "Gestión de productos", Font = new Font(Font.FontFamily, 20, FontStyle.Bold), ForeColor = ElementosInterfaz.Azul, AutoSize = true });
        cabecera.Controls.Add(new Label { Text = "Registrá un proveedor y luego cargá tus productos. Seleccioná una fila para modificarla.", AutoSize = true });
        raiz.Controls.Add(cabecera, 0, 0);

        var editor = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 4 };
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 118));
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        for (var i = 0; i < 4; i++) editor.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
        ElementosInterfaz.Campo(editor, "Código *", codigo, 0, 0);
        ElementosInterfaz.Campo(editor, "Nombre *", nombre, 2, 0);
        ElementosInterfaz.Campo(editor, "Categoría *", categoria, 0, 1);
        ElementosInterfaz.Campo(editor, "Proveedor *", proveedor, 2, 1);
        ElementosInterfaz.Campo(editor, "Precio *", precio, 0, 2);
        ElementosInterfaz.Campo(editor, "Stock", stock, 2, 2);
        ElementosInterfaz.Campo(editor, "Stock mínimo", stockMinimo, 0, 3);
        editor.Controls.Add(modo, 2, 3);
        editor.SetColumnSpan(modo, 2);
        raiz.Controls.Add(editor, 0, 1);

        var acciones = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        acciones.Controls.Add(ElementosInterfaz.Boton("Nuevo", (_, _) => NuevoProducto()));
        guardar = ElementosInterfaz.Boton("Guardar", (_, _) => ElementosInterfaz.Ejecutar(this, GuardarProducto), true);
        baja = ElementosInterfaz.Boton("Dar de baja", (_, _) => ElementosInterfaz.Ejecutar(this, DarDeBaja));
        acciones.Controls.Add(guardar);
        acciones.Controls.Add(baja);
        acciones.Controls.Add(ElementosInterfaz.Boton("Proveedores", (_, _) => ElementosInterfaz.Ejecutar(this, AbrirProveedores)));
        raiz.Controls.Add(acciones, 0, 2);

        var filtros = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        filtros.Controls.Add(new Label { Text = "Buscar:", AutoSize = true, Margin = new Padding(0, 10, 8, 0) });
        busqueda.Margin = new Padding(0, 6, 8, 0);
        filtros.Controls.Add(busqueda);
        filtros.Controls.Add(ElementosInterfaz.Boton("Consultar", (_, _) => ConsultarProductos()));
        filtros.Controls.Add(incluirInactivos);
        raiz.Controls.Add(filtros, 0, 3);
        incluirInactivos.CheckedChanged += (_, _) => ConsultarProductos();
        busqueda.KeyDown += (_, evento) =>
        {
            if (evento.KeyCode != Keys.Enter) return;
            evento.SuppressKeyPress = true;
            ConsultarProductos();
        };

        ElementosInterfaz.Columna(grilla, nameof(ProductoFila.Codigo), "Código", 75);
        ElementosInterfaz.Columna(grilla, nameof(ProductoFila.Nombre), "Nombre", 160);
        ElementosInterfaz.Columna(grilla, nameof(ProductoFila.Categoria), "Categoría", 110);
        ElementosInterfaz.Columna(grilla, nameof(ProductoFila.Precio), "Precio", 80, "N2");
        ElementosInterfaz.Columna(grilla, nameof(ProductoFila.StockDisponible), "Stock", 60);
        ElementosInterfaz.Columna(grilla, nameof(ProductoFila.StockMinimo), "Mínimo", 60);
        ElementosInterfaz.Columna(grilla, nameof(ProductoFila.Proveedor), "Proveedor", 135);
        ElementosInterfaz.Columna(grilla, nameof(ProductoFila.Estado), "Estado", 80);
        grilla.SelectionChanged += (_, _) =>
        {
            if (!actualizandoGrilla && grilla.SelectedRows.Count > 0)
                ElementosInterfaz.Ejecutar(this, SeleccionarProducto);
        };
        raiz.Controls.Add(grilla, 0, 4);
        raiz.Controls.Add(estado, 0, 5);

        Load += (_, _) => ElementosInterfaz.Ejecutar(this, () =>
        {
            CargarProveedores();
            CargarProductos();
            NuevoProducto();
        });
    }

    private void CargarProveedores(int? seleccionarId = null)
    {
        proveedor.DisplayMember = nameof(Proveedor.Nombre);
        proveedor.ValueMember = nameof(Proveedor.Id);
        proveedor.DataSource = proveedores.Consultar();
        proveedor.SelectedIndex = -1;
        if (seleccionarId.HasValue) proveedor.SelectedValue = seleccionarId.Value;
    }

    private void CargarProductos()
    {
        actualizandoGrilla = true;
        try
        {
            var filas = productos.Consultar(busqueda.Text, incluirInactivos.Checked).Select(p => new ProductoFila(p)).ToList();
            grilla.DataSource = filas;
            grilla.ClearSelection();
            foreach (DataGridViewRow fila in grilla.Rows)
            {
                var producto = ((ProductoFila)fila.DataBoundItem).Producto;
                if (!producto.Activo) fila.DefaultCellStyle.ForeColor = Color.Gray;
                else if (producto.StockDisponible <= producto.StockMinimo)
                    fila.DefaultCellStyle.BackColor = Color.FromArgb(255, 247, 219);
            }
            estado.Text = $"{filas.Count} producto(s). Las filas amarillas indican stock bajo.";
        }
        finally { actualizandoGrilla = false; }
    }

    private void ConsultarProductos() => ElementosInterfaz.Ejecutar(this, () =>
    {
        CargarProductos();
        NuevoProducto();
    });

    private void NuevoProducto()
    {
        productoId = 0;
        codigo.Clear();
        nombre.Clear();
        categoria.Clear();
        precio.Value = 0;
        stock.Value = 0;
        stockMinimo.Value = 0;
        proveedor.SelectedIndex = -1;
        grilla.ClearSelection();
        modo.Text = "Nuevo producto · * campos obligatorios";
        guardar.Enabled = true;
        baja.Enabled = false;
        codigo.Focus();
    }

    private void SeleccionarProducto()
    {
        var producto = ((ProductoFila)grilla.SelectedRows[0].DataBoundItem).Producto;
        productoId = producto.Id;
        codigo.Text = producto.Codigo;
        nombre.Text = producto.Nombre;
        categoria.Text = producto.Categoria;
        precio.Value = producto.Precio;
        stock.Value = producto.StockDisponible;
        stockMinimo.Value = producto.StockMinimo;
        CargarProveedores(producto.ProveedorId);
        modo.Text = producto.Activo ? $"Editando producto #{producto.Id}" : "Producto inactivo · solo consulta";
        guardar.Enabled = producto.Activo;
        baja.Enabled = producto.Activo;
    }

    private void GuardarProducto()
    {
        var producto = new Producto
        {
            Id = productoId,
            Codigo = codigo.Text,
            Nombre = nombre.Text,
            Categoria = categoria.Text,
            Precio = precio.Value,
            StockDisponible = (int)stock.Value,
            StockMinimo = (int)stockMinimo.Value,
            ProveedorId = proveedor.SelectedItem is Proveedor seleccionado ? seleccionado.Id : 0
        };
        productos.Guardar(producto);
        CargarProductos();
        NuevoProducto();
        estado.Text = $"Producto {producto.Codigo} guardado correctamente.";
    }

    private void DarDeBaja()
    {
        if (productoId == 0) throw new ArgumentException("Seleccioná un producto del listado.");
        var respuesta = MessageBox.Show(this, $"¿Dar de baja a {nombre.Text}?\nEl producto se conservará como inactivo.",
            "Confirmar baja", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
        if (respuesta != DialogResult.Yes) return;
        productos.DarDeBaja(productoId);
        CargarProductos();
        NuevoProducto();
        estado.Text = "Producto dado de baja correctamente.";
    }

    private void AbrirProveedores()
    {
        var proveedorId = proveedor.SelectedItem is Proveedor seleccionado ? seleccionado.Id : (int?)null;
        using var formulario = new ProveedoresForm(proveedores);
        formulario.ShowDialog(this);
        CargarProveedores(proveedorId);
        CargarProductos();
    }

    private sealed class ProductoFila
    {
        public Producto Producto { get; }
        public string Codigo => Producto.Codigo;
        public string Nombre => Producto.Nombre;
        public string Categoria => Producto.Categoria;
        public decimal Precio => Producto.Precio;
        public int StockDisponible => Producto.StockDisponible;
        public int StockMinimo => Producto.StockMinimo;
        public string Proveedor => Producto.Proveedor.Nombre;
        public string Estado => Producto.Activo ? "Activo" : "Inactivo";
        public ProductoFila(Producto producto) => Producto = producto;
    }
}
