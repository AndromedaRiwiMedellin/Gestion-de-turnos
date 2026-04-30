using Microsoft.EntityFrameworkCore;
using ShiftManagement.Models;

namespace ShiftManagement.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Turn> Turns { get; set; }
}