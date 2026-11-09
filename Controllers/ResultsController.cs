
using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace RaceDayMVC.Controllers;

public class ResultsController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ResultsController(IHttpClientFactory httpClientFactory)
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

    // Participant views their results
    public async Task<IActionResult> My()
    {
        if (HttpContext.Session.GetString("Role") != "Participant")
            return RedirectToAction("Login", "Auth");

        var client = GetApiClient();
        var response = await client.GetAsync("api/Results/my");

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error = "Unable to load your results.";
            return View(new List<MyResult>());
        }

        var results =
            await response.Content.ReadFromJsonAsync<List<MyResult>>();

        return View(results ?? new List<MyResult>());
    }

    // Organiser opens result form
    public IActionResult Create()
    {
        if (HttpContext.Session.GetString("Role") != "Organiser")
            return RedirectToAction("Login", "Auth");

        return View(new ResultViewModel());
    }

    // Organiser submits a result
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ResultViewModel model)
    {
        if (HttpContext.Session.GetString("Role") != "Organiser")
            return RedirectToAction("Login", "Auth");

        if (!ModelState.IsValid)
            return View(model);

        var client = GetApiClient();

        var payload = new
        {
            model.EnrolmentID,
            model.FinishTime,
            Position = model.Position
        };

        var response = await client.PostAsJsonAsync(
            "api/Results", payload);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("",
                await response.Content.ReadAsStringAsync());

            return View(model);
        }

        TempData["Success"] = "Result recorded successfully.";

        return RedirectToAction("Index", "Organiser");
    }
}

public class MyResult
{
    public int ResultID { get; set; }
    public string EventName { get; set; } = "";
    public TimeSpan FinishTime { get; set; }
    public int FinishingPosition { get; set; }
}
