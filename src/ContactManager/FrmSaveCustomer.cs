using ContactManager.Helpers;
using ContactManager.Models;
using ContactManager.Services;
using Microsoft.Extensions.Logging;

namespace ContactManager;

public sealed class FrmSaveCustomer : Form
{
    private readonly ILogger<FrmSaveCustomer> logger;
    private readonly ICustomerService service;
    private readonly ErrorProvider errors = new();
    private readonly PersonFormFields personFields = new();

    private readonly TextBox txtCompanyName = new();
    private readonly TextBox txtBusinessAddress = new();
    private readonly ComboBox cboCustomerType = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox txtCompanyContact = new();

    private int id;

    public bool HasChanged { get; private set; }

    public FrmSaveCustomer(ILogger<FrmSaveCustomer> logger, ICustomerService service)
    {
        this.logger = logger;
        this.service = service;
        errors.ContainerControl = this;

        Text = "Kunde speichern";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(760, 820);
        MinimumSize = new Size(680, 650);

        cboCustomerType.Items.AddRange(new object[] { "A", "B", "C", "D", "E" });
        cboCustomerType.SelectedIndex = 0;

        BuildUi();
        Load += FrmSaveCustomer_Load;
    }

    public void SetParams(int? customerId = null)
    {
        id = customerId ?? 0;
    }

    private void BuildUi()
    {
        var table = UiHelper.CreateFormTable();
        personFields.AddTo(table, socialSecurityRequired: false);
        UiHelper.AddRow(table, "Firmenname", txtCompanyName, true);
        UiHelper.AddRow(table, "Geschäftsadresse", txtBusinessAddress, true);
        UiHelper.AddRow(table, "Kundentyp (A-E)", cboCustomerType, true);
        UiHelper.AddRow(table, "Firmenkontakt", txtCompanyContact, true);

        var panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        panel.Controls.Add(table);

        var btnSave = new Button { Text = "Speichern", Width = 120, Height = 34 };
        var btnCancel = new Button { Text = "Abbrechen", Width = 120, Height = 34, DialogResult = DialogResult.Cancel };
        btnSave.Click += btnSave_Click;
        btnCancel.Click += (_, _) => Close();

        Controls.Add(panel);
        Controls.Add(UiHelper.CreateButtonBar(btnCancel, btnSave));
        AcceptButton = btnSave;
        CancelButton = btnCancel;
    }

    private void FrmSaveCustomer_Load(object? sender, EventArgs e)
    {
        TryExecute(() =>
        {
            if (id == 0)
            {
                Text = "Neuen Kunden erfassen";
                return;
            }

            var customer = service.GetOne(id);
            Text = "Kunde bearbeiten";
            personFields.Load(customer);
            txtCompanyName.Text = customer.CompanyName;
            txtBusinessAddress.Text = customer.BusinessAddress;
            var typeIndex = cboCustomerType.FindStringExact(customer.CustomerType.ToString());
            cboCustomerType.SelectedIndex = typeIndex >= 0 ? typeIndex : 0;
            txtCompanyContact.Text = customer.CompanyContact;
        });
    }

    private void btnSave_Click(object? sender, EventArgs e)
    {
        TryExecute(() =>
        {
            if (!ValidateInputs())
            {
                MessageBox.Show("Bitte korrigieren Sie die markierten Eingaben.", "Validierung",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var customer = new Customer { Id = id };
            personFields.ApplyTo(customer);
            customer.CompanyName = txtCompanyName.Text.Trim();
            customer.BusinessAddress = txtBusinessAddress.Text.Trim();
            customer.CustomerType = cboCustomerType.Text[0];
            customer.CompanyContact = txtCompanyContact.Text.Trim();

            if (id == 0)
            {
                service.AddOne(customer);
                logger.LogInformation("Kunde erstellt: {Customer}", JsonHelper.Serialize(customer));
            }
            else
            {
                service.UpdateOne(customer);
                logger.LogInformation("Kunde geändert: {Customer}", JsonHelper.Serialize(customer));
            }

            HasChanged = true;
            DialogResult = DialogResult.OK;
            Close();
        });
    }

    private bool ValidateInputs()
    {
        errors.Clear();
        var valid = personFields.Validate(errors, socialSecurityRequired: false);

        valid &= Require(txtCompanyName, "Firmenname ist ein Pflichtfeld.");
        valid &= Require(txtBusinessAddress, "Geschäftsadresse ist ein Pflichtfeld.");
        valid &= Require(txtCompanyContact, "Firmenkontakt ist ein Pflichtfeld.");

        if (cboCustomerType.Text.Length != 1 || "ABCDE".IndexOf(cboCustomerType.Text[0]) < 0)
        {
            errors.SetError(cboCustomerType, "Kundentyp muss A, B, C, D oder E sein.");
            valid = false;
        }

        return valid;
    }

    private bool Require(TextBox control, string message)
    {
        if (string.IsNullOrWhiteSpace(control.Text))
        {
            errors.SetError(control, message);
            return false;
        }

        errors.SetError(control, string.Empty);
        return true;
    }

    private void TryExecute(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fehler im Kundenformular");
            MessageBox.Show($"Ein Fehler ist aufgetreten:\n{ex.Message}", "Fehler",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
