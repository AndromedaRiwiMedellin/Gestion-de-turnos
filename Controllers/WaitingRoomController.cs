using Microsoft.AspNetCore.Mvc;

namespace ShiftManagement.Controllers;

public class WaitingRoomController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}