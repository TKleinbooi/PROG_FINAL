
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDayAPI.Services;
using System.Security.Claims;

namespace RaceDayAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UploadsController : ControllerBase
{
    private readonly RaceDayContext _context;
    private readonly AzureStorageService _storageService;

    public UploadsController(
        RaceDayContext context,
        AzureStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    [HttpPost("event-banner/{eventId}")]
    [Authorize(Roles = "Organiser")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadEventBanner(
        int eventId,
        IFormFile file)
    {
        var userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var eventItem = await _context.Events
            .FirstOrDefaultAsync(e =>
                e.EventID == eventId &&
                e.OrganiserID == userId);

        if (eventItem == null)
            return NotFound("Event not found or you do not own this event.");

        try
        {
            var imageUrl = await _storageService.UploadImageAsync(
                file, "event-banners");

            eventItem.BannerUrl = imageUrl;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Event banner uploaded successfully.",
                bannerUrl = imageUrl
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("profile-picture")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadProfilePicture(
        IFormFile file)
    {
        var userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserID == userId);

        if (user == null)
            return NotFound("User not found.");

        try
        {
            var imageUrl = await _storageService.UploadImageAsync(
                file, "profile-pictures");

            user.ProfilePictureUrl = imageUrl;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile picture uploaded successfully.",
                profilePictureUrl = imageUrl
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
