using ContactManager.Models;

namespace ContactManager.ViewModels;

public sealed class CustomerListItemVM
{
    public int Id { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public char CustomerType { get; init; }
    public bool IsActive { get; init; }

    public static CustomerListItemVM From(Customer customer) => new()
    {
        Id = customer.Id,
        CompanyName = customer.CompanyName,
        FirstName = customer.FirstName,
        LastName = customer.LastName,
        CustomerType = customer.CustomerType,
        IsActive = customer.IsActive
    };
}
