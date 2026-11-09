
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace RaceDayMVC.Controllers;

public class UploadsController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public UploadsController(IHttpClientFactory httpClientFactory)
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadEventBanner(
        int eventId, IFormFile file)
    {
        if (HttpContext.Session.GetString("Role") != "Organiser")
            return RedirectToAction("Login", "Auth");

        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Please select an image.";
            return RedirectToAction("Index", "Events");
        }

        using var content = new MultipartFormDataContent();
        await using var stream = file.OpenReadStream();

        using var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType =
            new MediaTypeHeaderValue(file.ContentType);

        content.Add(fileContent, "file", file.FileName);

        var client = GetApiClient();

        var response = await client.PostAsync(
            $"api/Uploads/event-banner/{eventId}", content);

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = await response.Content.ReadAsStringAsync();
        }
        else
        {
            TempData["Success"] = "Event banner uploaded successfully.";
        }

        return RedirectToAction("Index", "Events");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWT")))
            return RedirectToAction("Login", "Auth");

        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Please select an image.";
            return RedirectToDashboard();
        }

        using var content = new MultipartFormDataContent();
        await using var stream = file.OpenReadStream();

        using var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType =
            new MediaTypeHeaderValue(file.ContentType);

        content.Add(fileContent, "file", file.FileName);

        var client = GetApiClient();

        var response = await client.PostAsync(
            "api/Uploads/profile-picture", content);

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = await response.Content.ReadAsStringAsync();
        }
        else
        {
            TempData["Success"] = "Profile picture uploaded successfully.";
        }

        return RedirectToDashboard();
    }

    private IActionResult RedirectToDashboard()
    {
        var role = HttpContext.Session.GetString("Role");

        if (role == "Organiser")
            return RedirectToAction("Index", "Organiser");

        return RedirectToAction("Index", "Participant");
    }
}
