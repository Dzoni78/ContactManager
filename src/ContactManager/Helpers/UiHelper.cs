namespace ContactManager.Helpers;

public static class UiHelper
{
    public static TableLayoutPanel CreateFormTable()
    {
        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            Padding = new Padding(12)
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 185));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        return table;
    }

    public static void AddRow(TableLayoutPanel table, string labelText, Control control, bool required = false)
    {
        var row = table.RowCount++;
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var label = new Label
        {
            Text = required ? $"{labelText} *" : labelText,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(3, 8, 8, 8)
        };

        control.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        control.Margin = new Padding(3, 5, 3, 5);
        if (control is TextBox textBox)
        {
            textBox.Width = 350;
        }
        else if (control is ComboBox comboBox)
        {
            comboBox.Width = 350;
        }

        table.Controls.Add(label, 0, row);
        table.Controls.Add(control, 1, row);
    }

    public static FlowLayoutPanel CreateButtonBar(params Control[] controls)
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 58,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(8),
            WrapContents = false
        };

        foreach (var control in controls)
        {
            control.Margin = new Padding(5);
            panel.Controls.Add(control);
        }

        return panel;
    }
}
