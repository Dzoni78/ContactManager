using ContactManager.Models;

namespace ContactManager.Services;

public interface ICustomerService
{
    IReadOnlyList<Customer> GetMany();
    Customer GetOne(int id);
    IReadOnlyList<Customer> Find(string text);
    void AddOne(Customer customer);
    void UpdateOne(Customer customer);
    void SetActive(int id, bool isActive);
    void DeleteOne(int id);
    IReadOnlyList<CustomerContactNote> GetNotes(int customerId);
    void AddNote(int customerId, string noteText);
}
