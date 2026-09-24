using ContactManager.Core.Exceptions;
using ContactManager.Infrastructure;
using ContactManager.Models;
using Microsoft.EntityFrameworkCore;

namespace ContactManager.Services;

public sealed class CustomerService : ICustomerService
{
    private readonly ApplicationDbContext dbContext;

    public CustomerService(ApplicationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public IReadOnlyList<Customer> GetMany() => dbContext.Customers
        .AsNoTracking()
        .OrderBy(c => c.CompanyName)
        .ThenBy(c => c.LastName)
        .ToList();

    public Customer GetOne(int id) => dbContext.Customers
        .AsNoTracking()
        .FirstOrDefault(c => c.Id == id)
        ?? throw new NoRecordFoundException("Kunde wurde nicht gefunden.");

    public IReadOnlyList<Customer> Find(string text)
    {
        var query = text.Trim();
        if (string.IsNullOrWhiteSpace(query))
        {
            return GetMany();
        }

        var noteCustomerIds = dbContext.CustomerContactNotes
            .AsNoTracking()
            .ToList()
            .Where(n => n.NoteText.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        n.CreatedAt.ToString("g").Contains(query, StringComparison.OrdinalIgnoreCase))
            .Select(n => n.CustomerId)
            .ToHashSet();

        return GetMany().Where(c =>
                BuildSearchText(c).Contains(query, StringComparison.OrdinalIgnoreCase) ||
                noteCustomerIds.Contains(c.Id))
            .ToList();
    }

    public void AddOne(Customer customer)
    {
        dbContext.Customers.Add(customer);
        dbContext.SaveChanges();
        dbContext.Entry(customer).State = EntityState.Detached;
    }

    public void UpdateOne(Customer customer)
    {
        if (!dbContext.Customers.AsNoTracking().Any(c => c.Id == customer.Id))
        {
            throw new NoRecordFoundException("Kunde wurde nicht gefunden.");
        }

        dbContext.Customers.Update(customer);
        dbContext.SaveChanges();
        dbContext.Entry(customer).State = EntityState.Detached;
    }

    public void SetActive(int id, bool isActive)
    {
        var customer = dbContext.Customers.FirstOrDefault(c => c.Id == id)
            ?? throw new NoRecordFoundException("Kunde wurde nicht gefunden.");
        customer.IsActive = isActive;
        dbContext.SaveChanges();
        dbContext.Entry(customer).State = EntityState.Detached;
    }

    public void DeleteOne(int id)
    {
        var customer = dbContext.Customers.FirstOrDefault(c => c.Id == id)
            ?? throw new NoRecordFoundException("Kunde wurde nicht gefunden.");
        dbContext.Customers.Remove(customer);
        dbContext.SaveChanges();
    }

    public IReadOnlyList<CustomerContactNote> GetNotes(int customerId)
    {
        if (!dbContext.Customers.AsNoTracking().Any(c => c.Id == customerId))
        {
            throw new NoRecordFoundException("Kunde wurde nicht gefunden.");
        }

        return dbContext.CustomerContactNotes
            .AsNoTracking()
            .Where(n => n.CustomerId == customerId)
            .OrderByDescending(n => n.CreatedAt)
            .ToList();
    }

    public void AddNote(int customerId, string noteText)
    {
        if (!dbContext.Customers.AsNoTracking().Any(c => c.Id == customerId))
        {
            throw new NoRecordFoundException("Kunde wurde nicht gefunden.");
        }

        if (string.IsNullOrWhiteSpace(noteText))
        {
            throw new ArgumentException("Die Notiz darf nicht leer sein.", nameof(noteText));
        }

        dbContext.CustomerContactNotes.Add(new CustomerContactNote
        {
            CustomerId = customerId,
            CreatedAt = DateTime.Now,
            NoteText = noteText.Trim()
        });
        dbContext.SaveChanges();
    }

    private static string BuildSearchText(Customer c) => string.Join(" ", new object?[]
    {
        c.Salutation, c.FirstName, c.LastName, c.DateOfBirth.ToString("d"),
        c.GenderText, c.Title, c.SocialSecurityNumber, c.PhoneNumberPrivate,
        c.PhoneNumberMobile, c.PhoneNumberBusiness, c.Email, c.StatusText,
        c.Nationality, c.Street, c.StreetNumber, c.ZipCode, c.Place,
        c.CompanyName, c.BusinessAddress, c.CustomerType, c.CompanyContact
    });
}
