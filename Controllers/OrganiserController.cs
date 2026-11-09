
using Microsoft.AspNetCore.Mvc;

namespace RaceDayMVC.Controllers;

public class OrganiserController : Controller
{
    public IActionResult Index()
    {
        var role = HttpContext.Session.GetString("Role");

        if (string.IsNullOrWhiteSpace(role))
        {
            TempData["Error"] = "Your login session was not found. Please log in again.";
            return RedirectToAction("Login", "Auth");
        }

        if (!string.Equals(role, "Organiser",
            StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "You do not have permission to access the organiser dashboard.";
            return RedirectToAction("Login", "Auth");
        }

        ViewBag.FirstName = HttpContext.Session.GetString("FirstName");

        return View();
    }
}
