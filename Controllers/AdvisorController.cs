using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftManagement.Data;
using ShiftManagement.Models;

namespace ShiftManagement.Controllers;

public class AdvisorController : Controller
{
    private readonly MysqlDbContext _context;
    private const int AdvisorId = 1;

    public AdvisorController(MysqlDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var waiting = await _context.Turns
            .Where(t => t.Status == Status.Waiting)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();

        var current = await _context.Turns
            .Include(t => t.User)
            .Where(t => t.Status == Status.Called || t.Status == Status.InProgress)
            .OrderBy(t => t.CreatedAt)
            .FirstOrDefaultAsync();

        var today = DateTime.UtcNow.Date;
        var history = await _context.Turns
            .Where(t => t.Status == Status.Finished && t.UpdatedAt.Date == today)
            .OrderByDescending(t => t.UpdatedAt)
            .Take(10)
            .ToListAsync();

        ViewBag.WaitingTurns = waiting;
        ViewBag.CurrentTurn = current;
        ViewBag.History = history;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CallNext()
    {
        bool busy = await _context.Turns
            .AnyAsync(t => t.Status == Status.Called || t.Status == Status.InProgress);

        if (busy)
        {
            TempData["Error"] = "Finaliza o cancela el turno actual antes de llamar al siguiente.";
            return RedirectToAction(nameof(Index));
        }

        var next = await _context.Turns
            .Where(t => t.Status == Status.Waiting)
            .OrderBy(t => t.CreatedAt)
            .FirstOrDefaultAsync();

        if (next == null)
        {
            TempData["Info"] = "No hay turnos en espera.";
            return RedirectToAction(nameof(Index));
        }

        next.Status = Status.Called;
        next.AdvisorId = AdvisorId;
        next.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartAttention(int id)
    {
        var turn = await _context.Turns.FindAsync(id);

        if (turn == null || turn.Status != Status.Called)
        {
            TempData["Error"] = "Turno no encontrado o no está en estado Llamado.";
            return RedirectToAction(nameof(Index));
        }

        turn.Status = Status.InProgress;
        turn.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FinishTurn(int id, string? message)
    {
        var turn = await _context.Turns.FindAsync(id);

        if (turn == null || turn.Status != Status.InProgress)
        {
            TempData["Error"] = "Turno no encontrado o no está en atención.";
            return RedirectToAction(nameof(Index));
        }

        turn.Status = Status.Finished;
        turn.Message = message?.Trim() ?? string.Empty;
        turn.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelTurn(int id)
    {
        var turn = await _context.Turns.FindAsync(id);

        if (turn == null || (turn.Status != Status.Called && turn.Status != Status.InProgress))
        {
            TempData["Error"] = "Turno no encontrado o no puede cancelarse desde su estado actual.";
            return RedirectToAction(nameof(Index));
        }

        turn.Status = Status.Cancelled;
        turn.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> EditUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(int id, string fullname, string phone, string email)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.Fullname = fullname.Trim();
        user.Phone = phone.Trim();
        user.Email = email.Trim();
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["Info"] = $"Usuario {user.Fullname} actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
