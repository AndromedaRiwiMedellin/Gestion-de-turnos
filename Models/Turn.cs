namespace ShiftManagement.Models;

public enum Status {Waiting, Called, InProgress, Finished, Cancelled};

public class Turn
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;  // <- agregado
    public bool IsPrinted { get; set; } = false;
    public string Message { get; set; } = string.Empty;
    public Status Status { get; set; }
    
    public int? AdvisorId { get; set; }
    public Advisor? Advisor { get; set; }
    
    public int UserId { get; set; }
    public User? User { get; set; }
    
    public int WaitingRoomId { get; set; }
    public WaitingRoom WaitingRoom { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}