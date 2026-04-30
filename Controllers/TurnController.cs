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
    public async Task<IActionResult> RequestTurn(DocumentType documentType, string documentNumber)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.DocumentType == documentType && u.DocumentNumber == documentNumber);

        if (user == null)
        {
            TempData["Warning"] = "You must register before requesting a turn.";
            return RedirectToAction("Register", "User");
        }

        bool hasActiveTurn = await _context.Turns
            .AnyAsync(t => t.UserId == user.Id &&
                           (t.Status == Status.Waiting || t.Status == Status.Called || t.Status == Status.InProgress));

        if (hasActiveTurn)
        {
            TempData["Error"] = "You already have an active turn.";
            return RedirectToAction("Index");
        }

        var turn = new Turn { UserId = user.Id, WaitingRoomId = 1, Status = Status.Waiting };
        _context.Turns.Add(turn);
        await _context.SaveChangesAsync();

        turn.Code = $"A-{turn.Id:D3}";
        await _context.SaveChangesAsync();

        return View("Ticket", turn.Code);
    }
}
