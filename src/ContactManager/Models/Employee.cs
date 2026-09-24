namespace ContactManager.Models;

public class Employee : Person
{
    public Guid EmployeeNumber { get; set; }
    public string Department { get; set; } = string.Empty;
    public DateTime StartDate { get; set; } = DateTime.Today;
    public DateTime? EndDate { get; set; }
    public int Employment { get; set; } = 100;
    public string Role { get; set; } = string.Empty;
    public int CadreLevel { get; set; }
}
