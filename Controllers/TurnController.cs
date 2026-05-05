using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftManagement.Data;
using ShiftManagement.Models;
using ShiftManagement.Services;

namespace ShiftManagement.Controllers;

public class TurnController : Controller
{
    private readonly MysqlDbContext _context;
    private readonly EmailService _emailService;

    public TurnController(MysqlDbContext context, EmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterPatient(
        DocumentType documentType,
        string? documentNumber,
        string? fullname,
        string? phone,
        string? email)
    {
        if (string.IsNullOrWhiteSpace(documentNumber))
        {
            ModelState.AddModelError(nameof(documentNumber), "El número de documento es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(fullname))
        {
            ModelState.AddModelError(nameof(fullname), "El nombre completo es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            ModelState.AddModelError(nameof(email), "El correo electrónico es obligatorio.");
        }

        if (!ModelState.IsValid)
        {
            return View("Index");
        }

        string cleanDocumentNumber = documentNumber!.Trim();
        string cleanFullname = fullname!.Trim();
        string cleanPhone = phone?.Trim() ?? string.Empty;
        string cleanEmail = email!.Trim();

        User? user = await _context.Users.FirstOrDefaultAsync(u =>
            u.DocumentType == documentType &&
            u.DocumentNumber == cleanDocumentNumber);

        if (user == null)
        {
            user = new User
            {
                DocumentType = documentType,
                DocumentNumber = cleanDocumentNumber,
                Fullname = cleanFullname,
                Phone = cleanPhone,
                Email = cleanEmail,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
        }
        else
        {
            user.Fullname = cleanFullname;
            user.Phone = cleanPhone;
            user.Email = cleanEmail;
            user.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        await _emailService.SendEmailRegister(user.Email, user.Fullname);

        return RedirectToAction(nameof(SelectTicket), new { userId = user.Id });
    }

    [HttpGet]
    public async Task<IActionResult> SelectTicket(int userId)
    {
        User? user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound();
        }

        Turn? activeTurn = await GetActiveTurnByUserAsync(user.Id);

        ViewBag.ActiveTurn = activeTurn;

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTicket(int userId, string? ticketType)
    {
        User? user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(user.Email))
        {
            TempData["Error"] = "El paciente debe tener un correo electrónico registrado antes de generar el turno.";
            return RedirectToAction(nameof(Index));
        }

        Turn? activeTurn = await GetActiveTurnByUserAsync(user.Id);

        if (activeTurn != null)
        {
            TempData["Info"] = "El paciente ya tiene un turno activo.";
            return RedirectToAction(nameof(Ticket), new { id = activeTurn.Id });
        }

        WaitingRoom? waitingRoom = await _context.WaitingRooms
            .AsNoTracking()
            .OrderBy(w => w.Id)
            .FirstOrDefaultAsync();

        if (waitingRoom == null)
        {
            waitingRoom = new WaitingRoom
            {
                Name = "Sala Principal",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.WaitingRooms.Add(waitingRoom);
            await _context.SaveChangesAsync();
        }

        string normalizedTicketType = NormalizeTicketType(ticketType);
        string code = await GenerateTurnCodeAsync(normalizedTicketType);

        DateTime now = DateTime.UtcNow;
        int waitingStatus = (int)Status.Waiting;

        await _context.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO Turns
            (
                Code,
                IsPrinted,
                Message,
                Status,
                UserId,
                WaitingRoomId,
                Email,
                CreatedAt,
                UpdatedAt
            )
            VALUES
            (
                {code},
                {false},
                {string.Empty},
                {waitingStatus},
                {user.Id},
                {waitingRoom.Id},
                {user.Email},
                {now},
                {now}
            );
        ");

        Turn? createdTurn = await _context.Turns
            .AsNoTracking()
            .Where(t => t.UserId == user.Id && t.Code == code)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync();

        if (createdTurn == null)
        {
            TempData["Error"] = "No se pudo encontrar el turno generado.";
            return RedirectToAction(nameof(Index));
        }
        
        await _emailService.SendEmailTicket(user.Email, user.Fullname, code);

        return RedirectToAction(nameof(Ticket), new { id = createdTurn.Id, generated = true });
    }

    [HttpGet]
    public async Task<IActionResult> Ticket(int? id, string? code)
    {
        IQueryable<Turn> query = _context.Turns
            .AsNoTracking()
            .Include(t => t.User)
            .Include(t => t.WaitingRoom);

        Turn? turn = null;

        if (id.HasValue && id.Value > 0)
        {
            turn = await query.FirstOrDefaultAsync(t => t.Id == id.Value);
        }
        else if (!string.IsNullOrWhiteSpace(code))
        {
            string cleanCode = code.Trim();

            turn = await query
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync(t => t.Code == cleanCode);
        }
        else
        {
            return RedirectToAction(nameof(Index));
        }

        if (turn == null)
        {
            return NotFound();
        }

        return View(turn);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RequestTurn()
    {
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult PrintTicket(int id)
    {
        const string DISPOSITIVO = "/dev/usb/lp0";

        try
        {
            var turn = _context.Turns
                .Include(t => t.User)
                .Include(t => t.WaitingRoom)
                .FirstOrDefault(t => t.Id == id);

            if (turn == null) return NotFound();

            using var ms = new System.IO.MemoryStream();

            ms.Write(new byte[] { 0x1B, 0x40 });
            ms.Write(new byte[] { 0x1B, 0x61, 0x01 });
            ms.Write(new byte[] { 0x1D, 0x21, 0x00 });
            ms.Write(System.Text.Encoding.ASCII.GetBytes("========================\n"));
            ms.Write(System.Text.Encoding.ASCII.GetBytes("  CLINICA - TURNOS\n"));
            ms.Write(System.Text.Encoding.ASCII.GetBytes("========================\n"));
            ms.Write(new byte[] { 0x1D, 0x21, 0x22 });
            ms.Write(System.Text.Encoding.ASCII.GetBytes($"  {turn.Code}  \n"));
            ms.Write(new byte[] { 0x1D, 0x21, 0x00 });
            ms.Write(System.Text.Encoding.ASCII.GetBytes("========================\n"));
            ms.Write(System.Text.Encoding.ASCII.GetBytes($"  {turn.User?.Fullname}\n"));
            ms.Write(System.Text.Encoding.ASCII.GetBytes($"  {turn.WaitingRoom?.Name}\n"));
            ms.Write(System.Text.Encoding.ASCII.GetBytes($"  {DateTime.Now:dd/MM/yyyy HH:mm}\n"));
            ms.Write(System.Text.Encoding.ASCII.GetBytes("  Espere su llamado\n"));
            ms.Write(System.Text.Encoding.ASCII.GetBytes("========================\n"));
            ms.Write(System.Text.Encoding.ASCII.GetBytes("\n\n\n"));
            ms.Write(new byte[] { 0x1D, 0x56, 0x00 });

            System.IO.File.WriteAllBytes(DISPOSITIVO, ms.ToArray());

            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    private async Task<Turn?> GetActiveTurnByUserAsync(int userId)
    {
        return await _context.Turns
            .AsNoTracking()
            .Where(t =>
                t.UserId == userId &&
                (
                    t.Status == Status.Waiting ||
                    t.Status == Status.Called ||
                    t.Status == Status.InProgress
                ))
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync();
    }

    private async Task<string> GenerateTurnCodeAsync(string ticketType)
    {
        string prefix = GetTicketPrefix(ticketType);

        DateTime startDate = DateTime.UtcNow.Date;
        DateTime endDate = startDate.AddDays(1);

        int totalTodayByType = await _context.Turns
            .AsNoTracking()
            .CountAsync(t =>
                t.CreatedAt >= startDate &&
                t.CreatedAt < endDate &&
                t.Code.StartsWith(prefix + "-"));

        return $"{prefix}-{totalTodayByType + 1:D3}";
    }

    private static string NormalizeTicketType(string? ticketType)
    {
        if (string.IsNullOrWhiteSpace(ticketType))
        {
            return "GENERAL";
        }

        return ticketType.Trim().ToUpperInvariant();
    }

    private static string GetTicketPrefix(string ticketType)
    {
        return ticketType switch
        {
            "PRIORITY" => "U",
            "LABORATORY" => "L",
            "MEDICAL_APPOINTMENT" => "C",
            "INFORMATION" => "I",
            _ => "G"
        };
    }
}