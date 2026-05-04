using Microsoft.EntityFrameworkCore;
using ShiftManagement.Models;

namespace ShiftManagement.Data;

public class MysqlDbContext : DbContext
{

    public MysqlDbContext(DbContextOptions<MysqlDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Advisor> Advisors { get; set; }
    public DbSet<Turn> Turns { get; set; }
    public DbSet<WaitingRoom> WaitingRooms { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(user =>
        {
            user.Property(u => u.Fullname).IsRequired();
            user.Property(u => u.DocumentNumber).IsRequired();
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
        
        modelBuilder.Entity<Turn>()
            .HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId);
        
        modelBuilder.Entity<Turn>()
            .HasOne(t => t.Advisor)
            .WithMany()
            .HasForeignKey(t => t.AdvisorId);

        modelBuilder.Entity<WaitingRoom>(waitingRoom =>
        {
            waitingRoom.HasMany(wr => wr.Turns).WithOne(t => t.WaitingRoom).HasForeignKey(t => t.WaitingRoomId);
        });
        
        modelBuilder.Entity<WaitingRoom>()           
            .HasOne(w => w.Advisor)
            .WithOne(a => a.WaitingRoom)
            .HasForeignKey<Advisor>(a => a.WaitingRoomId);
        
    }
}