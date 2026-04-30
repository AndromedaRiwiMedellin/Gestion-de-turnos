using System;

namespace ShiftManagement.Models;

public class Turn
{
    public int Id { get; set; }
    public string Code { get; set; } // e.g., A-001
    public bool IsPrinted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}