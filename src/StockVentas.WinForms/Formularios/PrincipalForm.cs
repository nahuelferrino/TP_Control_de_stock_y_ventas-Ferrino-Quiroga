using AccesoDatos.Servicios;

namespace StockVentas.WinForms.Formularios;

public class PrincipalForm : Form
{
    public PrincipalForm(ProductoServicio productos, ProveedorServicio proveedores,
        ClienteServicio clientes, VentaServicio ventas, ReporteServicio reportes)
    {
        Text = "Stock y Ventas";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(820, 560);
        Size = new Size(980, 640);
        Font = new Font("Segoe UI", 11);
        BackColor = Color.FromArgb(245, 247, 250);
        AutoScaleMode = AutoScaleMode.Dpi;
        var raiz = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(25), ColumnCount = 2, RowCount = 2 };
        raiz.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 225));
        raiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        raiz.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        raiz.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(raiz);
        var titulo = new Label { Text = "Stock y Ventas", Font = new Font(Font.FontFamily, 26, FontStyle.Bold), ForeColor = ElementosInterfaz.Azul, AutoSize = true, Margin = new Padding(3, 0, 3, 20) };
        raiz.Controls.Add(titulo, 0, 0);
        raiz.SetColumnSpan(titulo, 2);
        var menu = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        void Seccion(string nombre, Func<Form> crear)
        {
            var boton = ElementosInterfaz.Boton(nombre, (_, _) => ElementosInterfaz.Ejecutar(this, () =>
            {
                using var formulario = crear();
                formulario.ShowDialog(this);
            }), true);
            boton.MinimumSize = new Size(195, 48);
            menu.Controls.Add(boton);
        }
        Seccion("Productos", () => new ProductosForm(productos, proveedores));
        Seccion("Proveedores", () => new ProveedoresForm(proveedores));
        Seccion("Clientes", () => new ClientesForm(clientes));
        Seccion("Ventas", () => new VentasForm(productos, clientes, ventas));
        Seccion("Reportes", () => new ReportesForm(reportes, clientes));
        menu.Controls.Add(ElementosInterfaz.Boton("Salir", (_, _) => Close()));
        raiz.Controls.Add(menu, 0, 1);
        var bienvenida = new Label
        {
            Text = "Organizá tu inventario y tus ventas\n\n"
                + "• Proveedores y productos: registrá qué vendés y controlá el stock.\n\n"
                + "• Clientes: mantené sus datos de contacto.\n\n"
                + "• Ventas: elegí el cliente, agregá productos y confirmá la compra.\n\n"
                + "• Reportes: consultá stock bajo, historial de compras, ventas por período, productos más vendidos e inventario por categoría.",
            Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(22, 10, 10, 10), BackColor = Color.White
        };
        var panelBienvenida = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.White };
        var contenidoBienvenida = new TableLayoutPanel
        {
            Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1, RowCount = 1
        };
        contenidoBienvenida.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        contenidoBienvenida.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        contenidoBienvenida.Controls.Add(bienvenida, 0, 0);
        panelBienvenida.Controls.Add(contenidoBienvenida);
        raiz.Controls.Add(panelBienvenida, 1, 1);
    }
}
