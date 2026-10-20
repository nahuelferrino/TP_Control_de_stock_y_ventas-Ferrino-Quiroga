using AccesoDatos.Modelos;
using AccesoDatos.Servicios;

namespace StockVentas.WinForms.Formularios;

public class ClientesForm : Form
{
    private readonly ClienteServicio clientes;
    private readonly TextBox nombre = new() { MaxLength = 100 };
    private readonly TextBox apellido = new() { MaxLength = 100 };
    private readonly TextBox telefono = new() { MaxLength = 30 };
    private readonly TextBox email = new() { MaxLength = 150 };
    private readonly CheckBox incluirInactivos = new() { Text = "Mostrar inactivos", AutoSize = true, Margin = new Padding(10, 10, 0, 0) };
    private readonly DataGridView grilla = ElementosInterfaz.Grilla();
    private readonly Label estado = new() { AutoSize = true, Dock = DockStyle.Fill, ForeColor = ElementosInterfaz.Azul };
    private readonly Button guardar;
    private readonly Button baja;
    private int clienteId;
    private bool actualizandoGrilla;

    public ClientesForm(ClienteServicio clientes)
    {
        this.clientes = clientes;
        Text = "Stock y Ventas — Clientes";
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
        raiz.Controls.Add(new Label { Text = "Gestión de clientes", AutoSize = true, Font = new Font(Font.FontFamily, 19, FontStyle.Bold), ForeColor = ElementosInterfaz.Azul }, 0, 0);
        var editor = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 2 };
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95));
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95));
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        editor.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        editor.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        ElementosInterfaz.Campo(editor, "Nombre *", nombre, 0, 0);
        ElementosInterfaz.Campo(editor, "Apellido *", apellido, 2, 0);
        ElementosInterfaz.Campo(editor, "Teléfono", telefono, 0, 1);
        ElementosInterfaz.Campo(editor, "Email", email, 2, 1);
        raiz.Controls.Add(editor, 0, 1);
        var acciones = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        acciones.Controls.Add(ElementosInterfaz.Boton("Nuevo", (_, _) => NuevoCliente()));
        guardar = ElementosInterfaz.Boton("Guardar", (_, _) => ElementosInterfaz.Ejecutar(this, GuardarCliente), true);
        baja = ElementosInterfaz.Boton("Dar de baja", (_, _) => ElementosInterfaz.Ejecutar(this, DarDeBaja));
        acciones.Controls.Add(guardar);
        acciones.Controls.Add(baja);
        acciones.Controls.Add(ElementosInterfaz.Boton("Consultar", (_, _) => ConsultarClientes()));
        acciones.Controls.Add(incluirInactivos);
        incluirInactivos.CheckedChanged += (_, _) => ConsultarClientes();
        raiz.Controls.Add(acciones, 0, 2);
        ElementosInterfaz.Columna(grilla, nameof(Cliente.Apellido), "Apellido", 120);
        ElementosInterfaz.Columna(grilla, nameof(Cliente.Nombre), "Nombre", 120);
        ElementosInterfaz.Columna(grilla, nameof(Cliente.Telefono), "Teléfono", 90);
        ElementosInterfaz.Columna(grilla, nameof(Cliente.Email), "Email", 150);
        grilla.Columns.Add(new DataGridViewCheckBoxColumn { DataPropertyName = nameof(Cliente.Activo), HeaderText = "Activo", FillWeight = 55 });
        grilla.SelectionChanged += (_, _) =>
        {
            if (!actualizandoGrilla && grilla.SelectedRows.Count > 0) SeleccionarCliente();
        };
        raiz.Controls.Add(grilla, 0, 3);
        raiz.Controls.Add(estado, 0, 4);
        Load += (_, _) => ConsultarClientes();
    }

    private void ConsultarClientes() => ElementosInterfaz.Ejecutar(this, () =>
    {
        actualizandoGrilla = true;
        try
        {
            grilla.DataSource = clientes.Consultar(incluirInactivos.Checked);
            grilla.ClearSelection();
        }
        finally { actualizandoGrilla = false; }
        NuevoCliente();
        estado.Text = $"{grilla.Rows.Count} cliente(s). * Campos obligatorios.";
    });

    private void NuevoCliente()
    {
        clienteId = 0;
        nombre.Clear(); apellido.Clear(); telefono.Clear(); email.Clear();
        grilla.ClearSelection();
        guardar.Enabled = true; baja.Enabled = false;
        nombre.Focus();
    }

    private void SeleccionarCliente()
    {
        var cliente = (Cliente)grilla.SelectedRows[0].DataBoundItem;
        clienteId = cliente.Id;
        nombre.Text = cliente.Nombre; apellido.Text = cliente.Apellido;
        telefono.Text = cliente.Telefono; email.Text = cliente.Email;
        guardar.Enabled = cliente.Activo; baja.Enabled = cliente.Activo;
        estado.Text = cliente.Activo ? $"Editando cliente #{cliente.Id}." : "Cliente inactivo · solo consulta.";
    }

    private void GuardarCliente()
    {
        clientes.Guardar(new Cliente { Id = clienteId, Nombre = nombre.Text, Apellido = apellido.Text, Telefono = telefono.Text, Email = email.Text });
        ConsultarClientes();
        estado.Text = "Cliente guardado correctamente.";
    }

    private void DarDeBaja()
    {
        if (clienteId == 0) throw new ArgumentException("Seleccioná un cliente del listado.");
        if (MessageBox.Show(this, $"¿Dar de baja a {nombre.Text} {apellido.Text}?\nSe conservará su historial de ventas.",
            "Confirmar baja", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
        clientes.DarDeBaja(clienteId);
        ConsultarClientes();
        estado.Text = "Cliente dado de baja correctamente.";
    }
}
