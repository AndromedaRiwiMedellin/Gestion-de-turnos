namespace ShiftManagement.Models;

public class User
{
    public int Id { get; set; }
    public string Document { get; set; } = string.Empty;
    public string Fullname { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public DateTime CreateAt { get; set; } = DateTime.Now;
    public DateTime UpdateAt { get; set; } = DateTime.Now;
}