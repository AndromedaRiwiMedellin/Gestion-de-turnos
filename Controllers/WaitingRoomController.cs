using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftManagement.Data;
using ShiftManagement.Models;

namespace ShiftManagement.Controllers;

public class WaitingRoomController : Controller
{
    private readonly MysqlDbContext _context;

    public WaitingRoomController(MysqlDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var currentTurn = await _context.Turns
            .Include(t => t.User)
            .Where(t => t.Status == Status.Called || t.Status == Status.InProgress)
            .OrderBy(t => t.CreatedAt)
            .FirstOrDefaultAsync();


        var queue = await _context.Turns
            .Where(t => t.Status == Status.Waiting)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();

        var lastCalledTurns = await _context.Turns
            .Where(t => t.Status == Status.Called || t.Status == Status.InProgress || t.Status == Status.Finished)
            .OrderByDescending(t => t.Id)
            .Take(5)
            .ToListAsync();

        if (currentTurn != null)
            lastCalledTurns = lastCalledTurns.Where(t => t.Id != currentTurn.Id).ToList();

        ViewBag.CurrentTurn = currentTurn;
        ViewBag.LastCalledTurns = lastCalledTurns;
        return View(queue);
    }
}