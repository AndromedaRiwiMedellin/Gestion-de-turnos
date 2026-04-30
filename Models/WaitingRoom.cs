namespace ShiftManagement.Models;

public class WaitingRoom
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    
    public Advisor Advisor { get; set; } = null!;
    
    public ICollection<Turn>? Turns { get; set; } = [];
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}