using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RaceDay.Api;

[ApiController, Route("api/images"), Authorize]
public class ImagesController(IImageStorage storage) : ControllerBase
{
    [HttpPost("event-banner"), Authorize(Roles = "Organiser")]
    public Task<IActionResult> EventBanner(IFormFile file, CancellationToken cancellationToken) => Upload(file, "banners", cancellationToken);

    [HttpPost("profile-picture"), Authorize(Roles = "Participant")]
    public Task<IActionResult> ProfilePicture(IFormFile file, CancellationToken cancellationToken) => Upload(file, "profiles", cancellationToken);

    private async Task<IActionResult> Upload(IFormFile file, string folder, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0) return BadRequest("Choose an image to upload.");
        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase) || file.Length > 5 * 1024 * 1024)
            return BadRequest("Upload an image smaller than 5 MB.");
        var url = await storage.UploadAsync(file, folder, cancellationToken);
        if (url.StartsWith('/')) url = $"{Request.Scheme}://{Request.Host}{url}";
        return Ok(new { url });
    }
}
