using ContactManager.Models;

namespace ContactManager.ViewModels;

public sealed class EmployeeListItemVM
{
    public int Id { get; init; }
    public Guid EmployeeNumber { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public bool IsActive { get; init; }

    public static EmployeeListItemVM From(Employee employee) => new()
    {
        Id = employee.Id,
        EmployeeNumber = employee.EmployeeNumber,
        FirstName = employee.FirstName,
        LastName = employee.LastName,
        Department = employee.Department,
        Role = employee.Role,
        Type = employee is Trainee ? "Lernender" : "Mitarbeiter",
        IsActive = employee.IsActive
    };
}
