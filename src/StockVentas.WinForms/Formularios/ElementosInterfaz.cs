using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace StockVentas.WinForms.Formularios;

internal static class ElementosInterfaz
{
    public static readonly Color Azul = Color.FromArgb(32, 67, 108);

    public static Button Boton(string texto, EventHandler accion, bool principal = false)
    {
        var boton = new Button
        {
            Text = texto,
            AutoSize = true,
            MinimumSize = new Size(105, 36),
            Padding = new Padding(10, 3, 10, 3),
            Margin = new Padding(0, 4, 8, 4),
            FlatStyle = FlatStyle.Flat,
            BackColor = principal ? Azul : Color.White,
            ForeColor = principal ? Color.White : Azul,
            UseVisualStyleBackColor = false
        };
        boton.Click += accion;
        return boton;
    }

    public static void Campo(TableLayoutPanel tabla, string etiqueta, Control control, int columna, int fila)
    {
        tabla.Controls.Add(new Label
        {
            Text = etiqueta,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 0, 8, 0)
        }, columna, fila);
        control.Dock = DockStyle.Fill;
        control.Margin = new Padding(0, 5, 15, 5);
        tabla.Controls.Add(control, columna + 1, fila);
    }

    public static DataGridView Grilla()
        => new()
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            RowTemplate = { Height = 30 }
        };

    public static void Columna(DataGridView grilla, string propiedad, string titulo, int peso = 100, string? formato = null)
    {
        var columna = new DataGridViewTextBoxColumn
        {
            DataPropertyName = propiedad,
            HeaderText = titulo,
            FillWeight = peso,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        if (formato is not null)
        {
            columna.DefaultCellStyle.Format = formato;
            columna.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }
        grilla.Columns.Add(columna);
    }

    public static void Ejecutar(Form formulario, Action accion)
    {
        try
        {
            accion();
        }
        catch (ArgumentException error)
        {
            MessageBox.Show(formulario, error.Message, "Revisá los datos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (InvalidOperationException error)
        {
            MessageBox.Show(formulario, error.Message, "No se pudo completar la operación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (DbUpdateException error)
        {
            Trace.WriteLine(error);
            MessageBox.Show(formulario, "No se pudieron guardar los cambios. Revisá que el código no esté repetido y que el proveedor siga disponible.",
                "Error al guardar", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception error)
        {
            Trace.WriteLine(error);
            MessageBox.Show(formulario, "No se pudo completar la operación. Verificá que la base de datos esté disponible y volvé a intentarlo.",
                "Stock y Ventas", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
