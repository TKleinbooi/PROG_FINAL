
using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDayMVC.Controllers;

public class AuthController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public IActionResult Login()
    {
        return View();
    }

    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");

            var response = await client.PostAsJsonAsync(
                "api/Auth/register", model);

            var responseText =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("",
                    "Registration failed: " + responseText);

                return View(model);
            }

            TempData["Success"] =
                "Registration successful! Please log in.";

            return RedirectToAction(nameof(Login));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("",
                "Registration error: " + ex.Message);

            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        Console.WriteLine("========== LOGIN STARTED ==========");

        if (!ModelState.IsValid)
        {
            Console.WriteLine("LOGIN MODEL IS INVALID");

            foreach (var error in ModelState.Values
                         .SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }

            return View(model);
        }

        try
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");

            Console.WriteLine("SENDING LOGIN REQUEST TO API");

            var response = await client.PostAsJsonAsync(
                "api/Auth/login", model);

            var responseText =
                await response.Content.ReadAsStringAsync();

            Console.WriteLine("API STATUS: " + response.StatusCode);
            Console.WriteLine("API RESPONSE: " + responseText);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("",
                    "Login failed: " + responseText);

                return View(model);
            }

            var result = JsonSerializer.Deserialize<LoginResponse>(
                responseText,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (result == null ||
                result.User == null ||
                string.IsNullOrWhiteSpace(result.Token))
            {
                ModelState.AddModelError("",
                    "Invalid login response from API.");

                return View(model);
            }

            // Save session information
            HttpContext.Session.SetString("JWT", result.Token);

            HttpContext.Session.SetString(
                "UserID", result.User.UserID.ToString());

            HttpContext.Session.SetString(
                "FirstName", result.User.FirstName);

            HttpContext.Session.SetString(
                "LastName", result.User.LastName);

            HttpContext.Session.SetString(
                "Email", result.User.Email);

            HttpContext.Session.SetString(
                "Role", result.User.Role);

            Console.WriteLine("SESSION ROLE: " +
                HttpContext.Session.GetString("Role"));

            // Redirect according to role
            if (string.Equals(
                result.User.Role,
                "Organiser",
                StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("REDIRECTING TO ORGANISER DASHBOARD");

                return RedirectToAction("Index", "Organiser");
            }

            if (string.Equals(
                result.User.Role,
                "Participant",
                StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("REDIRECTING TO PARTICIPANT DASHBOARD");

                return RedirectToAction("Index", "Participant");
            }

            ModelState.AddModelError("",
                "Unknown role: " + result.User.Role);

            return View(model);
        }
        catch (Exception ex)
        {
            Console.WriteLine("LOGIN EXCEPTION: " + ex);

            ModelState.AddModelError("",
                "Login error: " + ex.Message);

            return View(model);
        }
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();

        return RedirectToAction(nameof(Login));
    }
}

// API Login Response
public class LoginResponse
{
    public string Token { get; set; } = "";

    public LoginUser? User { get; set; }
}

// API User Information
public class LoginUser
{
    public int UserID { get; set; }

    public string FirstName { get; set; } = "";

    public string LastName { get; set; } = "";

    public string Email { get; set; } = "";

    public string Role { get; set; } = "";
}
