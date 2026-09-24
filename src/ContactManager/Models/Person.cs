using System.ComponentModel.DataAnnotations.Schema;

namespace ContactManager.Models;


/// Gemeinsame Basis für Kunden, Mitarbeitende und Lernende.

public abstract class Person
{
    public int Id { get; set; }
    public string Salutation { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; } = DateTime.Today;

    /// <summary>true = männlich, false = weiblich
    public bool Gender { get; set; }

    public string Title { get; set; } = string.Empty;
    public string SocialSecurityNumber { get; set; } = string.Empty;
    public string PhoneNumberPrivate { get; set; } = string.Empty;
    public string PhoneNumberMobile { get; set; } = string.Empty;
    public string PhoneNumberBusiness { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string Nationality { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string StreetNumber { get; set; } = string.Empty;
    public int ZipCode { get; set; }
    public string Place { get; set; } = string.Empty;

    [NotMapped]
    public string FullName => $"{FirstName} {LastName}".Trim();

    [NotMapped]
    public string StatusText => IsActive ? "Aktiv" : "Passiv";

    [NotMapped]
    public string GenderText => Gender ? "Männlich" : "Weiblich";
}
