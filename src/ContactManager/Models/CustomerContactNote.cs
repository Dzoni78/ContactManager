namespace ContactManager.Models;

public class CustomerContactNote
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string NoteText { get; set; } = string.Empty;
    public Customer Customer { get; set; } = null!;
}
