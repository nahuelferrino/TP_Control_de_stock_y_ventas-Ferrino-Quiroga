using AccesoDatos.Modelos;
using AccesoDatos.Servicios;

namespace StockVentas.WinForms.Formularios;

public class ProveedoresForm : Form
{
    private readonly ProveedorServicio proveedores;
    private readonly TextBox nombre = new() { MaxLength = 100 };
    private readonly TextBox telefono = new() { MaxLength = 30 };
    private readonly TextBox email = new() { MaxLength = 150 };
    private readonly TextBox direccion = new() { MaxLength = 200 };
    private readonly CheckBox incluirInactivos = new() { Text = "Mostrar inactivos", AutoSize = true, Margin = new Padding(10, 10, 0, 0) };
    private readonly DataGridView grilla = ElementosInterfaz.Grilla();
    private readonly Label estado = new() { AutoSize = true, Dock = DockStyle.Fill, ForeColor = ElementosInterfaz.Azul };
    private readonly Button guardar;
    private readonly Button baja;
    private int proveedorId;
    private bool actualizandoGrilla;

    public ProveedoresForm(ProveedorServicio proveedores)
    {
        this.proveedores = proveedores;
        Text = "Stock y Ventas — Proveedores";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(850, 530);
        Size = new Size(980, 630);
        Font = new Font("Segoe UI", 10);
        BackColor = Color.FromArgb(245, 247, 250);
        AutoScaleMode = AutoScaleMode.Dpi;
        var raiz = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), ColumnCount = 1, RowCount = 5 };
        raiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
        raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 95));
        raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
        raiz.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        Controls.Add(raiz);
        raiz.Controls.Add(new Label { Text = "Gestión de proveedores", AutoSize = true, Font = new Font(Font.FontFamily, 19, FontStyle.Bold), ForeColor = ElementosInterfaz.Azul }, 0, 0);

        var editor = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 2 };
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95));
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 85));
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        editor.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        editor.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        ElementosInterfaz.Campo(editor, "Nombre *", nombre, 0, 0);
        ElementosInterfaz.Campo(editor, "Teléfono", telefono, 2, 0);
        ElementosInterfaz.Campo(editor, "Email", email, 0, 1);
        ElementosInterfaz.Campo(editor, "Dirección", direccion, 2, 1);
        raiz.Controls.Add(editor, 0, 1);

        var acciones = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        acciones.Controls.Add(ElementosInterfaz.Boton("Nuevo", (_, _) => NuevoProveedor()));
        guardar = ElementosInterfaz.Boton("Guardar", (_, _) => ElementosInterfaz.Ejecutar(this, GuardarProveedor), true);
        baja = ElementosInterfaz.Boton("Dar de baja", (_, _) => ElementosInterfaz.Ejecutar(this, DarDeBaja));
        acciones.Controls.Add(guardar);
        acciones.Controls.Add(baja);
        acciones.Controls.Add(ElementosInterfaz.Boton("Consultar", (_, _) => ConsultarProveedores()));
        acciones.Controls.Add(incluirInactivos);
        incluirInactivos.CheckedChanged += (_, _) => ConsultarProveedores();
        raiz.Controls.Add(acciones, 0, 2);

        ElementosInterfaz.Columna(grilla, nameof(Proveedor.Nombre), "Nombre", 130);
        ElementosInterfaz.Columna(grilla, nameof(Proveedor.Telefono), "Teléfono", 90);
        ElementosInterfaz.Columna(grilla, nameof(Proveedor.Email), "Email", 140);
        ElementosInterfaz.Columna(grilla, nameof(Proveedor.Direccion), "Dirección", 140);
        var columnaEstado = new DataGridViewCheckBoxColumn { DataPropertyName = nameof(Proveedor.Activo), HeaderText = "Activo", FillWeight = 55 };
        grilla.Columns.Add(columnaEstado);
        grilla.SelectionChanged += (_, _) =>
        {
            if (!actualizandoGrilla && grilla.SelectedRows.Count > 0) SeleccionarProveedor();
        };
        raiz.Controls.Add(grilla, 0, 3);
        raiz.Controls.Add(estado, 0, 4);
        Load += (_, _) => ConsultarProveedores();
    }

    private void CargarProveedores()
    {
        actualizandoGrilla = true;
        try
        {
            grilla.DataSource = proveedores.Consultar(incluirInactivos.Checked);
            grilla.ClearSelection();
            estado.Text = $"{grilla.Rows.Count} proveedor(es). Solo el nombre es obligatorio.";
        }
        finally { actualizandoGrilla = false; }
    }

    private void ConsultarProveedores() => ElementosInterfaz.Ejecutar(this, () =>
    {
        CargarProveedores();
        NuevoProveedor();
    });

    private void NuevoProveedor()
    {
        proveedorId = 0;
        nombre.Clear();
        telefono.Clear();
        email.Clear();
        direccion.Clear();
        grilla.ClearSelection();
        guardar.Enabled = true;
        baja.Enabled = false;
        nombre.Focus();
    }

    private void SeleccionarProveedor()
    {
        var proveedor = (Proveedor)grilla.SelectedRows[0].DataBoundItem;
        proveedorId = proveedor.Id;
        nombre.Text = proveedor.Nombre;
        telefono.Text = proveedor.Telefono;
        email.Text = proveedor.Email;
        direccion.Text = proveedor.Direccion;
        guardar.Enabled = proveedor.Activo;
        baja.Enabled = proveedor.Activo;
        estado.Text = proveedor.Activo ? $"Editando proveedor #{proveedor.Id}." : "Proveedor inactivo · solo consulta.";
    }

    private void GuardarProveedor()
    {
        var proveedor = new Proveedor { Id = proveedorId, Nombre = nombre.Text, Telefono = telefono.Text, Email = email.Text, Direccion = direccion.Text };
        proveedores.Guardar(proveedor);
        CargarProveedores();
        NuevoProveedor();
        estado.Text = "Proveedor guardado correctamente.";
    }

    private void DarDeBaja()
    {
        if (proveedorId == 0) throw new ArgumentException("Seleccioná un proveedor del listado.");
        var respuesta = MessageBox.Show(this, $"¿Dar de baja a {nombre.Text}?\nLos productos asociados se conservarán. El proveedor ya no podrá seleccionarse al guardar productos.",
            "Confirmar baja", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
        if (respuesta != DialogResult.Yes) return;
        proveedores.DarDeBaja(proveedorId);
        CargarProveedores();
        NuevoProveedor();
        estado.Text = "Proveedor dado de baja correctamente.";
    }
}
