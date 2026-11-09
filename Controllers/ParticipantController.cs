
using Microsoft.AspNetCore.Mvc;

namespace RaceDayMVC.Controllers;

public class ParticipantController : Controller
{
    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("Role") != "Participant")
            return RedirectToAction("Login", "Auth");

        ViewBag.FirstName = HttpContext.Session.GetString("FirstName");

        return View();
    }
}
