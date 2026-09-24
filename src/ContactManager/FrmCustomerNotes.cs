using ContactManager.Services;
using Microsoft.Extensions.Logging;

namespace ContactManager;

public sealed class FrmCustomerNotes : Form
{
    private readonly ILogger<FrmCustomerNotes> logger;
    private readonly ICustomerService service;
    private readonly DataGridView dgvNotes = new();
    private readonly TextBox txtNewNote = new();
    private int customerId;

    public FrmCustomerNotes(ILogger<FrmCustomerNotes> logger, ICustomerService service)
    {
        this.logger = logger;
        this.service = service;

        Text = "Kundenkontakt-Historie";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(760, 620);
        MinimumSize = new Size(650, 500);
        BuildUi();
        Load += FrmCustomerNotes_Load;
    }

    public void SetParams(int id)
    {
        customerId = id;
    }

    private void BuildUi()
    {
        dgvNotes.Dock = DockStyle.Fill;
        dgvNotes.ReadOnly = true;
        dgvNotes.AllowUserToAddRows = false;
        dgvNotes.AllowUserToDeleteRows = false;
        dgvNotes.AutoGenerateColumns = false;
        dgvNotes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvNotes.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "CreatedAt",
            HeaderText = "Datum / Uhrzeit",
            Width = 160,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "g" }
        });
        dgvNotes.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "NoteText",
            HeaderText = "Notiz",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });

        var editor = new Panel { Dock = DockStyle.Bottom, Height = 150, Padding = new Padding(10) };
        var label = new Label { Text = "Neue Kontaktnotiz", Dock = DockStyle.Top, Height = 24 };
        txtNewNote.Multiline = true;
        txtNewNote.ScrollBars = ScrollBars.Vertical;
        txtNewNote.Dock = DockStyle.Fill;
        var btnAdd = new Button { Text = "Notiz speichern", Dock = DockStyle.Bottom, Height = 34 };
        btnAdd.Click += btnAdd_Click;
        editor.Controls.Add(txtNewNote);
        editor.Controls.Add(btnAdd);
        editor.Controls.Add(label);

        Controls.Add(dgvNotes);
        Controls.Add(editor);
    }

    private void FrmCustomerNotes_Load(object? sender, EventArgs e)
    {
        TryExecute(() =>
        {
            var customer = service.GetOne(customerId);
            Text = $"Kontakthistorie – {customer.CompanyName} / {customer.FullName}";
            BindNotes();
        });
    }

    private void btnAdd_Click(object? sender, EventArgs e)
    {
        TryExecute(() =>
        {
            if (string.IsNullOrWhiteSpace(txtNewNote.Text))
            {
                MessageBox.Show("Bitte zuerst eine Notiz eingeben.", "Hinweis",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            service.AddNote(customerId, txtNewNote.Text);
            logger.LogInformation("Kontaktnotiz für Kunde {CustomerId} gespeichert.", customerId);
            txtNewNote.Clear();
            BindNotes();
        });
    }

    private void BindNotes()
    {
        dgvNotes.DataSource = service.GetNotes(customerId).ToList();
    }

    private void TryExecute(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fehler in der Kundenkontakthistorie");
            MessageBox.Show($"Ein Fehler ist aufgetreten:\n{ex.Message}", "Fehler",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
