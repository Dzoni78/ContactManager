using System.Text;
using ContactManager.Models;
using ContactManager.Services;
using ContactManager.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ContactManager;

public sealed class FrmMain : Form
{
    private readonly ILogger<FrmMain> logger;
    private readonly IEmployeeService employeeService;
    private readonly ICustomerService customerService;

    private readonly DataGridView dgvEmployees = CreateGrid();
    private readonly DataGridView dgvCustomers = CreateGrid();
    private readonly TextBox txtEmployeeSearch = new();
    private readonly TextBox txtCustomerSearch = new();
    private readonly TextBox txtEmployeeDetails = CreateDetailsBox();
    private readonly TextBox txtCustomerDetails = CreateDetailsBox();

    private readonly Button btnEditEmployee = new() { Text = "Bearbeiten", Enabled = false };
    private readonly Button btnToggleEmployee = new() { Text = "Aktivieren/Deaktivieren", Enabled = false };
    private readonly Button btnDeleteEmployee = new() { Text = "Löschen", Enabled = false };
    private readonly Button btnEditCustomer = new() { Text = "Bearbeiten", Enabled = false };
    private readonly Button btnToggleCustomer = new() { Text = "Aktivieren/Deaktivieren", Enabled = false };
    private readonly Button btnDeleteCustomer = new() { Text = "Löschen", Enabled = false };
    private readonly Button btnCustomerNotes = new() { Text = "Kontakthistorie", Enabled = false };

    private int? selectedEmployeeId;
    private int? selectedCustomerId;

    public FrmMain(
        ILogger<FrmMain> logger,
        IEmployeeService employeeService,
        ICustomerService customerService)
    {
        this.logger = logger;
        this.employeeService = employeeService;
        this.customerService = customerService;

        Text = "Contact Manager";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(1350, 780);
        MinimumSize = new Size(1050, 650);

        BuildUi();
        Load += FrmMain_Load;
    }

    private void BuildUi()
    {
        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(BuildEmployeeTab());
        tabs.TabPages.Add(BuildCustomerTab());
        Controls.Add(tabs);
    }

    private TabPage BuildEmployeeTab()
    {
        ConfigureEmployeeGrid();
        var btnNew = new Button { Text = "Neu" };
        btnNew.Click += (_, _) => OpenEmployeeEditor(null);
        btnEditEmployee.Click += (_, _) => OpenEmployeeEditor(selectedEmployeeId);
        btnToggleEmployee.Click += (_, _) => ToggleEmployee();
        btnDeleteEmployee.Click += (_, _) => DeleteEmployee();
        txtEmployeeSearch.TextChanged += (_, _) => TryExecute(BindEmployees);
        dgvEmployees.SelectionChanged += (_, _) => TryExecute(EmployeeSelectionChanged);

        return BuildManagementTab(
            "Mitarbeiter",
            txtEmployeeSearch,
            dgvEmployees,
            txtEmployeeDetails,
            new[] { btnNew, btnEditEmployee, btnToggleEmployee, btnDeleteEmployee });
    }

    private TabPage BuildCustomerTab()
    {
        ConfigureCustomerGrid();
        var btnNew = new Button { Text = "Neu" };
        btnNew.Click += (_, _) => OpenCustomerEditor(null);
        btnEditCustomer.Click += (_, _) => OpenCustomerEditor(selectedCustomerId);
        btnToggleCustomer.Click += (_, _) => ToggleCustomer();
        btnDeleteCustomer.Click += (_, _) => DeleteCustomer();
        btnCustomerNotes.Click += (_, _) => OpenCustomerNotes();
        txtCustomerSearch.TextChanged += (_, _) => TryExecute(BindCustomers);
        dgvCustomers.SelectionChanged += (_, _) => TryExecute(CustomerSelectionChanged);

        return BuildManagementTab(
            "Kunden",
            txtCustomerSearch,
            dgvCustomers,
            txtCustomerDetails,
            new[] { btnNew, btnEditCustomer, btnToggleCustomer, btnDeleteCustomer, btnCustomerNotes });
    }

    private static TabPage BuildManagementTab(
        string title,
        TextBox search,
        DataGridView grid,
        TextBox details,
        IReadOnlyList<Button> buttons)
    {
        var tab = new TabPage(title);
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3,
            Padding = new Padding(8)
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 67));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));

        var searchPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        searchPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
        searchPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        searchPanel.Controls.Add(new Label { Text = "Suchen", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 0);
        search.Dock = DockStyle.Fill;
        search.Margin = new Padding(3, 8, 3, 8);
        searchPanel.Controls.Add(search, 1, 0);

        var detailGroup = new GroupBox { Text = "Details", Dock = DockStyle.Fill, Padding = new Padding(8) };
        detailGroup.Controls.Add(details);

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 8, 0, 0)
        };
        foreach (var button in buttons)
        {
            button.AutoSize = true;
            button.MinimumSize = new Size(100, 34);
            button.Margin = new Padding(4);
            buttonPanel.Controls.Add(button);
        }

        root.Controls.Add(searchPanel, 0, 0);
        root.SetColumnSpan(searchPanel, 2);
        root.Controls.Add(grid, 0, 1);
        root.Controls.Add(detailGroup, 1, 1);
        root.Controls.Add(buttonPanel, 0, 2);
        root.SetColumnSpan(buttonPanel, 2);
        tab.Controls.Add(root);
        return tab;
    }

    private void FrmMain_Load(object? sender, EventArgs e)
    {
        logger.LogInformation("Contact Manager gestartet.");
        TryExecute(() =>
        {
            BindEmployees();
            BindCustomers();
        });
    }

    private void ConfigureEmployeeGrid()
    {
        dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(EmployeeListItemVM.EmployeeNumber), HeaderText = "Mitarbeiternummer", Width = 220 });
        dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(EmployeeListItemVM.FirstName), HeaderText = "Vorname", Width = 120 });
        dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(EmployeeListItemVM.LastName), HeaderText = "Nachname", Width = 130 });
        dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(EmployeeListItemVM.Department), HeaderText = "Abteilung", Width = 130 });
        dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(EmployeeListItemVM.Role), HeaderText = "Rolle", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        dgvEmployees.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(EmployeeListItemVM.Type), HeaderText = "Typ", Width = 100 });
        dgvEmployees.Columns.Add(new DataGridViewCheckBoxColumn { DataPropertyName = nameof(EmployeeListItemVM.IsActive), HeaderText = "Aktiv", Width = 60 });
    }

    private void ConfigureCustomerGrid()
    {
        dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(CustomerListItemVM.CompanyName), HeaderText = "Firma", Width = 180 });
        dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(CustomerListItemVM.FirstName), HeaderText = "Vorname", Width = 120 });
        dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(CustomerListItemVM.LastName), HeaderText = "Nachname", Width = 130 });
        dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(CustomerListItemVM.CustomerType), HeaderText = "Typ", Width = 60 });
        dgvCustomers.Columns.Add(new DataGridViewCheckBoxColumn { DataPropertyName = nameof(CustomerListItemVM.IsActive), HeaderText = "Aktiv", Width = 60 });
    }

    private void BindEmployees()
    {
        var employees = string.IsNullOrWhiteSpace(txtEmployeeSearch.Text)
            ? employeeService.GetMany()
            : employeeService.Find(txtEmployeeSearch.Text);

        dgvEmployees.DataSource = employees.Select(EmployeeListItemVM.From).ToList();
        if (dgvEmployees.Rows.Count == 0)
        {
            ClearEmployeeSelection();
        }
    }

    private void BindCustomers()
    {
        var customers = string.IsNullOrWhiteSpace(txtCustomerSearch.Text)
            ? customerService.GetMany()
            : customerService.Find(txtCustomerSearch.Text);

        dgvCustomers.DataSource = customers.Select(CustomerListItemVM.From).ToList();
        if (dgvCustomers.Rows.Count == 0)
        {
            ClearCustomerSelection();
        }
    }

    private void EmployeeSelectionChanged()
    {
        if (dgvEmployees.CurrentRow?.DataBoundItem is not EmployeeListItemVM row)
        {
            ClearEmployeeSelection();
            return;
        }

        selectedEmployeeId = row.Id;
        var employee = employeeService.GetOne(row.Id);
        txtEmployeeDetails.Text = FormatEmployee(employee);
        btnEditEmployee.Enabled = true;
        btnToggleEmployee.Enabled = true;
        btnDeleteEmployee.Enabled = true;
        btnToggleEmployee.Text = employee.IsActive ? "Deaktivieren" : "Aktivieren";
    }

    private void CustomerSelectionChanged()
    {
        if (dgvCustomers.CurrentRow?.DataBoundItem is not CustomerListItemVM row)
        {
            ClearCustomerSelection();
            return;
        }

        selectedCustomerId = row.Id;
        var customer = customerService.GetOne(row.Id);
        txtCustomerDetails.Text = FormatCustomer(customer);
        btnEditCustomer.Enabled = true;
        btnToggleCustomer.Enabled = true;
        btnDeleteCustomer.Enabled = true;
        btnCustomerNotes.Enabled = true;
        btnToggleCustomer.Text = customer.IsActive ? "Deaktivieren" : "Aktivieren";
    }

    private void OpenEmployeeEditor(int? id)
    {
        TryExecute(() =>
        {
            var form = Program.ServiceProvider.GetRequiredService<FrmSaveEmployee>();
            form.SetParams(id);
            form.ShowDialog(this);
            if (form.HasChanged)
            {
                BindEmployees();
            }
        });
    }

    private void OpenCustomerEditor(int? id)
    {
        TryExecute(() =>
        {
            var form = Program.ServiceProvider.GetRequiredService<FrmSaveCustomer>();
            form.SetParams(id);
            form.ShowDialog(this);
            if (form.HasChanged)
            {
                BindCustomers();
            }
        });
    }

    private void ToggleEmployee()
    {
        if (!selectedEmployeeId.HasValue) return;
        TryExecute(() =>
        {
            var employee = employeeService.GetOne(selectedEmployeeId.Value);
            employeeService.SetActive(employee.Id, !employee.IsActive);
            logger.LogInformation("Mitarbeiter {EmployeeId}: Status auf {Status} gesetzt.", employee.Id, !employee.IsActive);
            BindEmployees();
        });
    }

    private void ToggleCustomer()
    {
        if (!selectedCustomerId.HasValue) return;
        TryExecute(() =>
        {
            var customer = customerService.GetOne(selectedCustomerId.Value);
            customerService.SetActive(customer.Id, !customer.IsActive);
            logger.LogInformation("Kunde {CustomerId}: Status auf {Status} gesetzt.", customer.Id, !customer.IsActive);
            BindCustomers();
        });
    }

    private void DeleteEmployee()
    {
        if (!selectedEmployeeId.HasValue) return;
        if (MessageBox.Show("Mitarbeiter endgültig löschen?", "Löschen",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

        TryExecute(() =>
        {
            employeeService.DeleteOne(selectedEmployeeId.Value);
            logger.LogWarning("Mitarbeiter {EmployeeId} gelöscht.", selectedEmployeeId.Value);
            BindEmployees();
        });
    }

    private void DeleteCustomer()
    {
        if (!selectedCustomerId.HasValue) return;
        if (MessageBox.Show("Kunde inklusive Kontaktnotizen endgültig löschen?", "Löschen",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

        TryExecute(() =>
        {
            customerService.DeleteOne(selectedCustomerId.Value);
            logger.LogWarning("Kunde {CustomerId} gelöscht.", selectedCustomerId.Value);
            BindCustomers();
        });
    }

    private void OpenCustomerNotes()
    {
        if (!selectedCustomerId.HasValue) return;
        TryExecute(() =>
        {
            var form = Program.ServiceProvider.GetRequiredService<FrmCustomerNotes>();
            form.SetParams(selectedCustomerId.Value);
            form.ShowDialog(this);
        });
    }

    private void ClearEmployeeSelection()
    {
        selectedEmployeeId = null;
        txtEmployeeDetails.Clear();
        btnEditEmployee.Enabled = false;
        btnToggleEmployee.Enabled = false;
        btnDeleteEmployee.Enabled = false;
    }

    private void ClearCustomerSelection()
    {
        selectedCustomerId = null;
        txtCustomerDetails.Clear();
        btnEditCustomer.Enabled = false;
        btnToggleCustomer.Enabled = false;
        btnDeleteCustomer.Enabled = false;
        btnCustomerNotes.Enabled = false;
    }

    private static DataGridView CreateGrid() => new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AutoGenerateColumns = false,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        MultiSelect = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        RowHeadersVisible = false,
        AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells
    };

    private static TextBox CreateDetailsBox() => new()
    {
        Dock = DockStyle.Fill,
        Multiline = true,
        ReadOnly = true,
        ScrollBars = ScrollBars.Vertical,
        BackColor = SystemColors.Window
    };

    private static string FormatEmployee(Employee e)
    {
        var sb = FormatPerson(e);
        sb.AppendLine($"Mitarbeiternummer: {e.EmployeeNumber}");
        sb.AppendLine($"Abteilung: {e.Department}");
        sb.AppendLine($"Eintritt: {e.StartDate:d}");
        sb.AppendLine($"Austritt: {(e.EndDate.HasValue ? e.EndDate.Value.ToString("d") : "-")}");
        sb.AppendLine($"Beschäftigungsgrad: {e.Employment}%");
        sb.AppendLine($"Rolle: {e.Role}");
        sb.AppendLine($"Kaderstufe: {e.CadreLevel}");
        if (e is Trainee trainee)
        {
            sb.AppendLine($"Lehrjahre: {trainee.TraineeYears}");
            sb.AppendLine($"Aktuelles Lehrjahr: {trainee.ActualTraineeYear}");
        }
        return sb.ToString();
    }

    private static string FormatCustomer(Customer c)
    {
        var sb = FormatPerson(c);
        sb.AppendLine($"Firma: {c.CompanyName}");
        sb.AppendLine($"Geschäftsadresse: {c.BusinessAddress}");
        sb.AppendLine($"Kundentyp: {c.CustomerType}");
        sb.AppendLine($"Firmenkontakt: {c.CompanyContact}");
        return sb.ToString();
    }

    private static StringBuilder FormatPerson(Person p)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Status: {p.StatusText}");
        sb.AppendLine($"Anrede: {p.Salutation}");
        sb.AppendLine($"Name: {p.FirstName} {p.LastName}");
        sb.AppendLine($"Geburtsdatum: {p.DateOfBirth:d}");
        sb.AppendLine($"Geschlecht: {p.GenderText}");
        sb.AppendLine($"Titel: {p.Title}");
        sb.AppendLine($"AHV-Nummer: {p.SocialSecurityNumber}");
        sb.AppendLine($"Telefon privat: {p.PhoneNumberPrivate}");
        sb.AppendLine($"Mobil: {p.PhoneNumberMobile}");
        sb.AppendLine($"Telefon Geschäft: {p.PhoneNumberBusiness}");
        sb.AppendLine($"E-Mail: {p.Email}");
        sb.AppendLine($"Nationalität: {p.Nationality}");
        sb.AppendLine($"Adresse: {p.Street} {p.StreetNumber}, {p.ZipCode} {p.Place}");
        sb.AppendLine();
        return sb;
    }

    private void TryExecute(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fehler im Hauptfenster");
            MessageBox.Show($"Ein Fehler ist aufgetreten:\n{ex.Message}", "Fehler",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
