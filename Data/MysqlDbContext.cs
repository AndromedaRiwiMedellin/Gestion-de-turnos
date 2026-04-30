using Microsoft.EntityFrameworkCore;
using ShiftManagement.Models;

namespace ShiftManagement.Data;

public class MysqlDbContext : DbContext
{

    public MysqlDbContext(DbContextOptions<MysqlDbContext> options) : base(options)
    {
    }

    private DbSet<User> Users { get; set; }
    private DbSet<Advisor> Advisors { get; set; }
    private DbSet<Turn> Turns { get; set; }
    private DbSet<WaitingRoom> WaitingRooms { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(user =>
        {
            user.Property(u => u.Fullname).IsRequired();
            user.Property(u => u.Document).IsRequired();
            user.Property(u => u.Email).IsRequired();
        });

        modelBuilder.Entity<Advisor>(advisor =>
        {
            advisor.Property(a => a.Fullname).IsRequired();
        });

        modelBuilder.Entity<Turn>(turn =>
        {
            turn.Property(t => t.Code).IsRequired();
            turn.Property(t => t.IsPrinted).IsRequired();
            turn.Property(t => t.Message).IsRequired();
            turn.Property(t => t.Status).IsRequired();
            turn.Property(t => t.UserId).IsRequired();
        });
    }
}