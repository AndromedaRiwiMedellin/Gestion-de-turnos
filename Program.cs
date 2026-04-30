using Microsoft.EntityFrameworkCore;
using ShiftManagement.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<MysqlDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MysqlDbContext>();
    try
    {
        if (!db.WaitingRooms.Any())
        {
            var room = new WaitingRoom { Name = "Sala Principal", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            db.WaitingRooms.Add(room);
            db.SaveChanges();
            db.Advisors.Add(new Advisor { Fullname = "Asesor 1", WaitingRoomId = room.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
            db.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Seed] Skipped: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
