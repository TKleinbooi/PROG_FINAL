
using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace RaceDayMVC.Controllers;

public class EnrolmentsController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public EnrolmentsController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private HttpClient GetApiClient()
    {
        var client = _httpClientFactory.CreateClient("RaceDayApi");
        var token = HttpContext.Session.GetString("JWT");

        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        return client;
    }

    // Participant enrolment form
    public IActionResult Create(int eventId)
    {
        if (HttpContext.Session.GetString("Role") != "Participant")
            return RedirectToAction("Login", "Auth");

        return View(new EnrolmentViewModel
        {
            EventID = eventId
        });
    }

    // Submit enrolment
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EnrolmentViewModel model)
    {
        if (HttpContext.Session.GetString("Role") != "Participant")
            return RedirectToAction("Login", "Auth");

        if (!ModelState.IsValid)
            return View(model);

        var client = GetApiClient();

        var payload = new
        {
            model.EventID,
            model.CategoryID
        };

        var response = await client.PostAsJsonAsync(
            "api/Enrolments", payload);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("",
                await response.Content.ReadAsStringAsync());

            return View(model);
        }

        TempData["Success"] = "You have successfully enrolled in the event.";

        return RedirectToAction("My");
    }

    // Participant views their enrolments
    public async Task<IActionResult> My()
    {
        if (HttpContext.Session.GetString("Role") != "Participant")
            return RedirectToAction("Login", "Auth");

        var client = GetApiClient();

        var response = await client.GetAsync("api/Enrolments/my");

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error = "Unable to load your enrolments.";
            return View(new List<MyEnrolment>());
        }

        var enrolments =
            await response.Content.ReadFromJsonAsync<List<MyEnrolment>>();

        return View(enrolments ?? new List<MyEnrolment>());
    }

    // Organiser views enrolments for a specific event
    public async Task<IActionResult> Event(int eventId)
    {
        if (HttpContext.Session.GetString("Role") != "Organiser")
            return RedirectToAction("Login", "Auth");

        var client = GetApiClient();

        var response = await client.GetAsync(
            $"api/Enrolments/event/{eventId}");

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = "Unable to load event enrolments.";
            return RedirectToAction("Index", "Events");
        }

        var enrolments =
            await response.Content.ReadFromJsonAsync<List<EventEnrolment>>();

        ViewBag.EventID = eventId;

        return View(enrolments ?? new List<EventEnrolment>());
    }
}

public class MyEnrolment
{
    public int EnrolmentID { get; set; }
    public string EventName { get; set; } = "";
    public int EventID { get; set; }
    public string Category { get; set; } = "";
    public DateTime EnrolmentDate { get; set; }
}

public class EventEnrolment
{
    public int EnrolmentID { get; set; }
    public int UserID { get; set; }
    public string Participant { get; set; } = "";
    public string CategoryName { get; set; } = "";
    public DateTime EnrolmentDate { get; set; }
}
