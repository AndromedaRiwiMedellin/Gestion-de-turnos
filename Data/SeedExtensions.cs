using ShiftManagement.Models;

namespace ShiftManagement.Data;

public static class SeedExtensions
{
    public static void SeedDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MysqlDbContext>();

        if (db.WaitingRooms.Any()) return;

        db.WaitingRooms.Add(new WaitingRoom
        {
            Name = "Sala Principal",
            Advisor = new Advisor { Fullname = "Asesor 1" }
        });

        db.SaveChanges();
    }
}
