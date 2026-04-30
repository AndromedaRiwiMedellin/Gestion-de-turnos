namespace ShiftManagement.Models;

public class Advisor
{
    public int Id { get; set; }
    public string Fullname { get; set; } = string.Empty;
    
    public DateTime CreateAt { get; set; } = DateTime.Now;
    public DateTime UpdateAt { get; set; } = DateTime.Now;
}