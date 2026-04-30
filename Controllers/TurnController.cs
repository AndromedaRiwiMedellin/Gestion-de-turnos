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

        var waitingRoom = await _context.WaitingRooms.FirstOrDefaultAsync();
        if (waitingRoom == null)
        {
            TempData["Error"] = "No waiting room is available.";
            return RedirectToAction("Index");
        }

        int totalToday = await _context.Turns
            .CountAsync(t => t.CreatedAt.Date == DateTime.Today);

        var turn = new Turn
        {
            Code = $"A-{(totalToday + 1):D3}",
            UserId = user.Id,
            WaitingRoomId = waitingRoom.Id,
            Status = Status.Waiting
        };

        _context.Turns.Add(turn);
        await _context.SaveChangesAsync();

        return View("Ticket", turn.Code);
    }
}
