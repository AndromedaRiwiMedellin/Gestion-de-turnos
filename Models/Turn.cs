namespace ShiftManagement.Models;

public enum Status {Waiting, Cancelled, Finished};

public class Turn
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public bool IsPrinted { get; set; } = false; // Tracks if the ticket was printed
    public string Message { get; set; } = string.Empty;
    public Status Status { get; set; }
    
    public int UserId { get; set; }
    public User User { get; set; }
 
    public DateTime CreateAt { get; set; } = DateTime.Now;
    public DateTime UpdateAt { get; set; } = DateTime.Now;
}