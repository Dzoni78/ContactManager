namespace ContactManager.Models;

public class Customer : Person
{
    public string CompanyName { get; set; } = string.Empty;
    public string BusinessAddress { get; set; } = string.Empty;
    public char CustomerType { get; set; } = 'E';
    public string CompanyContact { get; set; } = string.Empty;
    public List<CustomerContactNote> ContactNotes { get; set; } = new();
}
