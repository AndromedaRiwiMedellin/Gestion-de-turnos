using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftManagement.Data;
using ShiftManagement.Models;

namespace ShiftManagement.Controllers;

public class UserController : Controller
{
    private readonly MysqlDbContext _context;

    public UserController(MysqlDbContext context)
    {
        _context = context;
    }

    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(User user)
    {
        bool exists = await _context.Users
            .AnyAsync(u => u.DocumentType == user.DocumentType && u.DocumentNumber == user.DocumentNumber);

        if (exists)
        {
            TempData["Error"] = "A user with this document already exists.";
            return View(user);
        }

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Registration successful. You can now request your turn.";
        return RedirectToAction("Index", "Turn");
    }
}
