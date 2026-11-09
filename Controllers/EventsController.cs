
using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace RaceDayMVC.Controllers;

public class EventsController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public EventsController(IHttpClientFactory httpClientFactory)
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

    private bool IsOrganiser()
    {
        return HttpContext.Session.GetString("Role") == "Organiser";
    }

    public async Task<IActionResult> Index()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWT")))
            return RedirectToAction("Login", "Auth");

        try
        {
            var response = await GetApiClient().GetAsync("api/Events");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Unable to load events: " +
                    await response.Content.ReadAsStringAsync();

                return View(new List<EventItem>());
            }

            var events = await response.Content
                .ReadFromJsonAsync<List<EventItem>>();

            return View(events ?? new List<EventItem>());
        }
        catch (HttpRequestException)
        {
            ViewBag.Error = "Could not connect to the RaceDay API.";
            return View(new List<EventItem>());
        }
    }

    [HttpGet]
    public IActionResult Create()
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Auth");

        return View(new EventViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventViewModel model)
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Auth");

        if (!ModelState.IsValid)
            return View(model);

        var payload = new
        {
            model.Name,
            model.Description,
            EventDate = model.EventDate,
            model.Location,
            model.Distance,
            model.EventType
        };

        try
        {
            var response = await GetApiClient()
                .PostAsJsonAsync("api/Events", payload);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("",
                    await response.Content.ReadAsStringAsync());

                return View(model);
            }

            TempData["Success"] = "Event created successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError("", "Unable to connect to the API.");
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Auth");

        try
        {
            var response = await GetApiClient()
                .GetAsync($"api/Events/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Unable to find the selected event.";
                return RedirectToAction(nameof(Index));
            }

            var item = await response.Content.ReadFromJsonAsync<EventItem>();

            if (item == null)
                return NotFound();

            var model = new EventViewModel
            {
                EventID = item.EventID,
                Name = item.Name,
                Description = item.Description,
                EventDate = item.Date,
                Location = item.Location,
                Distance = item.Distance,
                EventType = item.EventType
            };

            return View(model);
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "Unable to connect to the API.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EventViewModel model)
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Auth");

        if (!ModelState.IsValid)
            return View(model);

        var payload = new
        {
            model.Name,
            model.Description,
            EventDate = model.EventDate,
            model.Location,
            model.Distance,
            model.EventType
        };

        try
        {
            var response = await GetApiClient()
                .PutAsJsonAsync($"api/Events/{model.EventID}", payload);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("",
                    await response.Content.ReadAsStringAsync());

                return View(model);
            }

            TempData["Success"] = "Event updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError("", "Unable to connect to the API.");
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Auth");

        try
        {
            var response = await GetApiClient()
                .DeleteAsync($"api/Events/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] =
                    await response.Content.ReadAsStringAsync();
            }
            else
            {
                TempData["Success"] = "Event deleted successfully!";
            }
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "Unable to connect to the API.";
        }

        return RedirectToAction(nameof(Index));
    }
}

public class EventItem
{
    public int EventID { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime Date { get; set; }
    public string Location { get; set; } = "";
    public decimal Distance { get; set; }
    public string EventType { get; set; } = "";
    public string? BannerUrl { get; set; }
}
