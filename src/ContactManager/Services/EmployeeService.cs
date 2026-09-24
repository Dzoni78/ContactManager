using ContactManager.Core.Exceptions;
using ContactManager.Infrastructure;
using ContactManager.Models;
using Microsoft.EntityFrameworkCore;

namespace ContactManager.Services;

public sealed class EmployeeService : IEmployeeService
{
    private readonly ApplicationDbContext dbContext;

    public EmployeeService(ApplicationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public IReadOnlyList<Employee> GetMany() => dbContext.Employees
        .AsNoTracking()
        .OrderBy(e => e.LastName)
        .ThenBy(e => e.FirstName)
        .ToList();

    public Employee GetOne(int id) => dbContext.Employees
        .AsNoTracking()
        .FirstOrDefault(e => e.Id == id)
        ?? throw new NoRecordFoundException("Mitarbeiter wurde nicht gefunden.");

    public IReadOnlyList<Employee> Find(string text)
    {
        var query = text.Trim();
        if (string.IsNullOrWhiteSpace(query))
        {
            return GetMany();
        }

        return GetMany().Where(e => BuildSearchText(e)
            .Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public void AddOne(Employee employee)
    {
        if (employee.EmployeeNumber == Guid.Empty)
        {
            employee.EmployeeNumber = Guid.NewGuid();
        }

        dbContext.Employees.Add(employee);
        dbContext.SaveChanges();
        dbContext.Entry(employee).State = EntityState.Detached;
    }

    public void UpdateOne(Employee employee)
    {
        var existing = dbContext.Employees.AsNoTracking()
            .FirstOrDefault(e => e.Id == employee.Id)
            ?? throw new NoRecordFoundException("Mitarbeiter wurde nicht gefunden.");

        // Die Mitarbeiternummer wird nie durch Benutzereingaben verändert.
        employee.EmployeeNumber = existing.EmployeeNumber;
        dbContext.Employees.Update(employee);
        dbContext.SaveChanges();
        dbContext.Entry(employee).State = EntityState.Detached;
    }

    public void SetActive(int id, bool isActive)
    {
        var employee = dbContext.Employees.FirstOrDefault(e => e.Id == id)
            ?? throw new NoRecordFoundException("Mitarbeiter wurde nicht gefunden.");
        employee.IsActive = isActive;
        dbContext.SaveChanges();
        dbContext.Entry(employee).State = EntityState.Detached;
    }

    public void DeleteOne(int id)
    {
        var employee = dbContext.Employees.FirstOrDefault(e => e.Id == id)
            ?? throw new NoRecordFoundException("Mitarbeiter wurde nicht gefunden.");
        dbContext.Employees.Remove(employee);
        dbContext.SaveChanges();
    }

    private static string BuildSearchText(Employee e)
    {
        var trainee = e as Trainee;
        return string.Join(" ", new object?[]
        {
            e.EmployeeNumber, e.Salutation, e.FirstName, e.LastName,
            e.DateOfBirth.ToString("d"), e.GenderText, e.Title,
            e.SocialSecurityNumber, e.PhoneNumberPrivate, e.PhoneNumberMobile,
            e.PhoneNumberBusiness, e.Email, e.StatusText, e.Nationality,
            e.Street, e.StreetNumber, e.ZipCode, e.Place, e.Department,
            e.StartDate.ToString("d"), e.EndDate?.ToString("d"), e.Employment,
            e.Role, e.CadreLevel, trainee?.TraineeYears, trainee?.ActualTraineeYear,
            trainee is null ? "Mitarbeiter" : "Lernender Trainee"
        });
    }
}
