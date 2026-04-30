using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftManagement.Data;
using ShiftManagement.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

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
        // Genera el código basado en los turnos de hoy
        int totalToday = await _context.Turns
            .CountAsync(t => t.CreatedAt.Date == DateTime.Today);
            
        string newCode = $"A-{(totalToday + 1).ToString("D3")}";

        var newTurn = new Turn { Code = newCode };
        
        _context.Turns.Add(newTurn);
        await _context.SaveChangesAsync();

        TempData["Message"] = $"Your turn is {newCode}. Please wait for your ticket.";
        return RedirectToAction("Index");
    }
}