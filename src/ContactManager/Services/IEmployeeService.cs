using ContactManager.Models;

namespace ContactManager.Services;

public interface IEmployeeService
{
    IReadOnlyList<Employee> GetMany();
    Employee GetOne(int id);
    IReadOnlyList<Employee> Find(string text);
    void AddOne(Employee employee);
    void UpdateOne(Employee employee);
    void SetActive(int id, bool isActive);
    void DeleteOne(int id);
}
