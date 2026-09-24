using ContactManager.Models;

namespace ContactManager.Helpers;


/// Gemeinsame WinForms-Eingabefelder für alle von Person
/// Dadurch bleiben Mitarbeiter- und Kundenformular konsistent.
public sealed class PersonFormFields
{
    public ComboBox Salutation { get; } = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    public TextBox FirstName { get; } = new();
    public TextBox LastName { get; } = new();
    public DateTimePicker DateOfBirth { get; } = new() { Format = DateTimePickerFormat.Short, MaxDate = DateTime.Today };
    public ComboBox Gender { get; } = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    public TextBox Title { get; } = new();
    public TextBox SocialSecurityNumber { get; } = new();
    public TextBox PhonePrivate { get; } = new();
    public TextBox PhoneMobile { get; } = new();
    public TextBox PhoneBusiness { get; } = new();
    public TextBox Email { get; } = new();
    public CheckBox IsActive { get; } = new() { Text = "Aktiv", Checked = true, AutoSize = true };
    public TextBox Nationality { get; } = new();
    public TextBox Street { get; } = new();
    public TextBox StreetNumber { get; } = new();
    public NumericUpDown ZipCode { get; } = new() { Minimum = 0, Maximum = 99999, ThousandsSeparator = false };
    public TextBox Place { get; } = new();

    public PersonFormFields()
    {
        Salutation.Items.AddRange(new object[] { "Herr", "Frau" });
        Salutation.SelectedIndex = 0;
        Gender.Items.AddRange(new object[] { "Männlich", "Weiblich" });
        Gender.SelectedIndex = 0;
    }

    public void AddTo(TableLayoutPanel table, bool socialSecurityRequired)
    {
        UiHelper.AddRow(table, "Anrede", Salutation, true);
        UiHelper.AddRow(table, "Vorname", FirstName, true);
        UiHelper.AddRow(table, "Nachname", LastName, true);
        UiHelper.AddRow(table, "Geburtsdatum", DateOfBirth, true);
        UiHelper.AddRow(table, "Geschlecht", Gender, true);
        UiHelper.AddRow(table, "Titel", Title);
        UiHelper.AddRow(table, "AHV-Nummer", SocialSecurityNumber, socialSecurityRequired);
        UiHelper.AddRow(table, "Telefon privat", PhonePrivate);
        UiHelper.AddRow(table, "Mobiltelefon", PhoneMobile);
        UiHelper.AddRow(table, "Telefon Geschäft", PhoneBusiness);
        UiHelper.AddRow(table, "E-Mail", Email, true);
        UiHelper.AddRow(table, "Status", IsActive);
        UiHelper.AddRow(table, "Nationalität", Nationality, true);
        UiHelper.AddRow(table, "Strasse", Street, true);
        UiHelper.AddRow(table, "Hausnummer", StreetNumber, true);
        UiHelper.AddRow(table, "PLZ", ZipCode, true);
        UiHelper.AddRow(table, "Wohnort", Place, true);
    }

    public void Load(Person person)
    {
        SelectComboValue(Salutation, person.Salutation);
        FirstName.Text = person.FirstName;
        LastName.Text = person.LastName;
        DateOfBirth.Value = ClampDate(person.DateOfBirth);
        Gender.SelectedIndex = person.Gender ? 0 : 1;
        Title.Text = person.Title;
        SocialSecurityNumber.Text = person.SocialSecurityNumber;
        PhonePrivate.Text = person.PhoneNumberPrivate;
        PhoneMobile.Text = person.PhoneNumberMobile;
        PhoneBusiness.Text = person.PhoneNumberBusiness;
        Email.Text = person.Email;
        IsActive.Checked = person.IsActive;
        Nationality.Text = person.Nationality;
        Street.Text = person.Street;
        StreetNumber.Text = person.StreetNumber;
        ZipCode.Value = Math.Clamp(person.ZipCode, (int)ZipCode.Minimum, (int)ZipCode.Maximum);
        Place.Text = person.Place;
    }

    public void ApplyTo(Person person)
    {
        person.Salutation = Salutation.Text.Trim();
        person.FirstName = FirstName.Text.Trim();
        person.LastName = LastName.Text.Trim();
        person.DateOfBirth = DateOfBirth.Value.Date;
        person.Gender = Gender.SelectedIndex == 0;
        person.Title = Title.Text.Trim();
        person.SocialSecurityNumber = SocialSecurityNumber.Text.Trim();
        person.PhoneNumberPrivate = PhonePrivate.Text.Trim();
        person.PhoneNumberMobile = PhoneMobile.Text.Trim();
        person.PhoneNumberBusiness = PhoneBusiness.Text.Trim();
        person.Email = Email.Text.Trim();
        person.IsActive = IsActive.Checked;
        person.Nationality = Nationality.Text.Trim();
        person.Street = Street.Text.Trim();
        person.StreetNumber = StreetNumber.Text.Trim();
        person.ZipCode = (int)ZipCode.Value;
        person.Place = Place.Text.Trim();
    }

    public bool Validate(ErrorProvider errors, bool socialSecurityRequired)
    {
        var valid = true;
        valid &= Require(errors, Salutation, "Bitte Anrede auswählen.");
        valid &= Require(errors, FirstName, "Vorname ist ein Pflichtfeld.");
        valid &= Require(errors, LastName, "Nachname ist ein Pflichtfeld.");
        valid &= Require(errors, Email, "E-Mail ist ein Pflichtfeld.");
        valid &= Require(errors, Nationality, "Nationalität ist ein Pflichtfeld.");
        valid &= Require(errors, Street, "Strasse ist ein Pflichtfeld.");
        valid &= Require(errors, StreetNumber, "Hausnummer ist ein Pflichtfeld.");
        valid &= Require(errors, Place, "Wohnort ist ein Pflichtfeld.");

        if (socialSecurityRequired)
        {
            valid &= Require(errors, SocialSecurityNumber, "AHV-Nummer ist ein Pflichtfeld.");
        }

        if (ZipCode.Value <= 0)
        {
            errors.SetError(ZipCode, "Bitte eine gültige Postleitzahl eingeben.");
            valid = false;
        }
        else
        {
            errors.SetError(ZipCode, string.Empty);
        }

        if (!string.IsNullOrWhiteSpace(Email.Text) && !ValidationHelper.IsValidEmail(Email.Text))
        {
            errors.SetError(Email, "Bitte eine gültige E-Mail-Adresse eingeben.");
            valid = false;
        }

        valid &= ValidatePhone(errors, PhonePrivate);
        valid &= ValidatePhone(errors, PhoneMobile);
        valid &= ValidatePhone(errors, PhoneBusiness);

        return valid;
    }

    private static bool Require(ErrorProvider errors, Control control, string message)
    {
        var text = control switch
        {
            TextBox box => box.Text,
            ComboBox combo => combo.Text,
            _ => string.Empty
        };

        if (string.IsNullOrWhiteSpace(text))
        {
            errors.SetError(control, message);
            return false;
        }

        errors.SetError(control, string.Empty);
        return true;
    }

    private static bool ValidatePhone(ErrorProvider errors, TextBox control)
    {
        if (!ValidationHelper.IsValidPhone(control.Text))
        {
            errors.SetError(control, "Telefonnummer enthält ungültige Zeichen.");
            return false;
        }

        errors.SetError(control, string.Empty);
        return true;
    }

    private static void SelectComboValue(ComboBox comboBox, string value)
    {
        var index = comboBox.FindStringExact(value);
        comboBox.SelectedIndex = index >= 0 ? index : 0;
    }

    private static DateTime ClampDate(DateTime value)
    {
        if (value < DateTimePicker.MinimumDateTime)
        {
            return DateTimePicker.MinimumDateTime;
        }

        return value > DateTime.Today ? DateTime.Today : value;
    }
}
