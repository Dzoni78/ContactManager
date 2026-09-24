using ContactManager.Helpers;
using ContactManager.Models;
using ContactManager.Services;
using Microsoft.Extensions.Logging;

namespace ContactManager;

public sealed class FrmSaveEmployee : Form
{
    private readonly ILogger<FrmSaveEmployee> logger;
    private readonly IEmployeeService service;
    private readonly ErrorProvider errors = new();
    private readonly PersonFormFields personFields = new();

    private readonly TextBox txtEmployeeNumber = new() { ReadOnly = true };
    private readonly TextBox txtDepartment = new();
    private readonly DateTimePicker dtpStartDate = new() { Format = DateTimePickerFormat.Short };
    private readonly DateTimePicker dtpEndDate = new() { Format = DateTimePickerFormat.Short, ShowCheckBox = true, Checked = false };
    private readonly NumericUpDown nudEmployment = new() { Minimum = 0, Maximum = 100, Value = 100 };
    private readonly TextBox txtRole = new();
    private readonly NumericUpDown nudCadreLevel = new() { Minimum = 0, Maximum = 5 };
    private readonly CheckBox chkTrainee = new() { Text = "Lernender / Trainee", AutoSize = true };
    private readonly NumericUpDown nudTraineeYears = new() { Minimum = 1, Maximum = 10, Value = 4 };
    private readonly NumericUpDown nudActualTraineeYear = new() { Minimum = 1, Maximum = 10, Value = 1 };

    private int id;
    private Employee? loadedEmployee;

    public bool HasChanged { get; private set; }

    public FrmSaveEmployee(ILogger<FrmSaveEmployee> logger, IEmployeeService service)
    {
        this.logger = logger;
        this.service = service;
        errors.ContainerControl = this;

        Text = "Mitarbeiter speichern";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(760, 850);
        MinimumSize = new Size(680, 650);

        BuildUi();
        Load += FrmSaveEmployee_Load;
    }

    public void SetParams(int? employeeId = null)
    {
        id = employeeId ?? 0;
    }

    private void BuildUi()
    {
        var table = UiHelper.CreateFormTable();
        personFields.AddTo(table, socialSecurityRequired: true);

        UiHelper.AddRow(table, "Mitarbeiternummer", txtEmployeeNumber);
        UiHelper.AddRow(table, "Abteilung", txtDepartment, true);
        UiHelper.AddRow(table, "Eintrittsdatum", dtpStartDate, true);
        UiHelper.AddRow(table, "Austrittsdatum", dtpEndDate);
        UiHelper.AddRow(table, "Beschäftigungsgrad %", nudEmployment, true);
        UiHelper.AddRow(table, "Rolle / Tätigkeit", txtRole, true);
        UiHelper.AddRow(table, "Kaderstufe (0-5)", nudCadreLevel, true);
        UiHelper.AddRow(table, "Lernender", chkTrainee);
        UiHelper.AddRow(table, "Lehrjahre", nudTraineeYears);
        UiHelper.AddRow(table, "Aktuelles Lehrjahr", nudActualTraineeYear);

        chkTrainee.CheckedChanged += (_, _) => UpdateTraineeControls();
        UpdateTraineeControls();

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

    private void FrmSaveEmployee_Load(object? sender, EventArgs e)
    {
        TryExecute(() =>
        {
            if (id == 0)
            {
                Text = "Neuen Mitarbeiter erfassen";
                txtEmployeeNumber.Text = "Wird automatisch vergeben";
                dtpStartDate.Value = DateTime.Today;
                return;
            }

            loadedEmployee = service.GetOne(id);
            Text = "Mitarbeiter bearbeiten";
            personFields.Load(loadedEmployee);
            txtEmployeeNumber.Text = loadedEmployee.EmployeeNumber.ToString();
            txtDepartment.Text = loadedEmployee.Department;
            dtpStartDate.Value = loadedEmployee.StartDate;
            dtpEndDate.Checked = loadedEmployee.EndDate.HasValue;
            if (loadedEmployee.EndDate.HasValue)
            {
                dtpEndDate.Value = loadedEmployee.EndDate.Value;
            }
            nudEmployment.Value = Math.Clamp(loadedEmployee.Employment, (int)nudEmployment.Minimum, (int)nudEmployment.Maximum);
            txtRole.Text = loadedEmployee.Role;
            nudCadreLevel.Value = Math.Clamp(loadedEmployee.CadreLevel, (int)nudCadreLevel.Minimum, (int)nudCadreLevel.Maximum);

            if (loadedEmployee is Trainee trainee)
            {
                chkTrainee.Checked = true;
                nudTraineeYears.Value = Math.Clamp(trainee.TraineeYears, (int)nudTraineeYears.Minimum, (int)nudTraineeYears.Maximum);
                nudActualTraineeYear.Value = Math.Clamp(trainee.ActualTraineeYear, (int)nudActualTraineeYear.Minimum, (int)nudActualTraineeYear.Maximum);
            }

            // Der Typ wird bei bestehenden Datensätzen nicht geändert, damit der TPH-Datensatz stabil bleibt.
            chkTrainee.Enabled = false;
            UpdateTraineeControls();
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

            Employee employee = chkTrainee.Checked ? new Trainee() : new Employee();
            employee.Id = id;
            employee.EmployeeNumber = loadedEmployee?.EmployeeNumber ?? Guid.Empty;
            personFields.ApplyTo(employee);
            employee.Department = txtDepartment.Text.Trim();
            employee.StartDate = dtpStartDate.Value.Date;
            employee.EndDate = dtpEndDate.Checked ? dtpEndDate.Value.Date : null;
            employee.Employment = (int)nudEmployment.Value;
            employee.Role = txtRole.Text.Trim();
            employee.CadreLevel = (int)nudCadreLevel.Value;

            if (employee is Trainee trainee)
            {
                trainee.TraineeYears = (int)nudTraineeYears.Value;
                trainee.ActualTraineeYear = (int)nudActualTraineeYear.Value;
            }

            if (id == 0)
            {
                service.AddOne(employee);
                logger.LogInformation("Mitarbeiter erstellt: {Employee}", JsonHelper.Serialize(employee));
            }
            else
            {
                service.UpdateOne(employee);
                logger.LogInformation("Mitarbeiter geändert: {Employee}", JsonHelper.Serialize(employee));
            }

            HasChanged = true;
            DialogResult = DialogResult.OK;
            Close();
        });
    }

    private bool ValidateInputs()
    {
        errors.Clear();
        var valid = personFields.Validate(errors, socialSecurityRequired: true);

        if (string.IsNullOrWhiteSpace(txtDepartment.Text))
        {
            errors.SetError(txtDepartment, "Abteilung ist ein Pflichtfeld.");
            valid = false;
        }

        if (string.IsNullOrWhiteSpace(txtRole.Text))
        {
            errors.SetError(txtRole, "Rolle ist ein Pflichtfeld.");
            valid = false;
        }

        if (nudEmployment.Value <= 0 || nudEmployment.Value > 100)
        {
            errors.SetError(nudEmployment, "Beschäftigungsgrad muss zwischen 1 und 100 liegen.");
            valid = false;
        }

        if (dtpEndDate.Checked && dtpEndDate.Value.Date < dtpStartDate.Value.Date)
        {
            errors.SetError(dtpEndDate, "Austrittsdatum darf nicht vor dem Eintrittsdatum liegen.");
            valid = false;
        }

        if (chkTrainee.Checked && nudActualTraineeYear.Value > nudTraineeYears.Value)
        {
            errors.SetError(nudActualTraineeYear, "Aktuelles Lehrjahr darf nicht grösser als die Anzahl Lehrjahre sein.");
            valid = false;
        }

        return valid;
    }

    private void UpdateTraineeControls()
    {
        nudTraineeYears.Enabled = chkTrainee.Checked;
        nudActualTraineeYear.Enabled = chkTrainee.Checked;
    }

    private void TryExecute(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fehler im Mitarbeiterformular");
            MessageBox.Show($"Ein Fehler ist aufgetreten:\n{ex.Message}", "Fehler",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
