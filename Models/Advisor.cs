namespace ShiftManagement.Models;

public class Advisor
{
    public int Id { get; set; }
    public string Fullname { get; set; } = string.Empty;
    
    public int WaitingRoomId { get; set; }
    public WaitingRoom WaitingRoom { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}