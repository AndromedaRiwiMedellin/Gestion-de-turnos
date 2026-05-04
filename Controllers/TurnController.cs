using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftManagement.Data;
using ShiftManagement.Models;

namespace ShiftManagement.Controllers;

public class TurnController : Controller
{
    private readonly MysqlDbContext _context;

    public TurnController(MysqlDbContext context)
    {
        _context = context;
    }

    public IActionResult Index() => View();

    [HttpPost]
    public async Task<IActionResult> RequestTurn()
    {
        int totalToday = await _context.Turns
            .CountAsync(t => t.CreatedAt.Date == DateTime.Today);

        string newCode = $"A-{(totalToday + 1).ToString("D3")}";

        var newTurn = new Turn { Code = newCode };
        _context.Turns.Add(newTurn);
        await _context.SaveChangesAsync();

        return RedirectToAction("Ticket", new { code = newCode });
    }

    public IActionResult Ticket(string code)
    {
        return View("Ticket", code);
    }
}