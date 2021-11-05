using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PigeonAPI;
using PigeonAPI.Models;

namespace PigeonAPI.Controllers;

/// <summary>
/// Class to handle all requests on the / route
/// </summary>
[ApiController]
[Route("[controller]")]
public class DataController : ControllerBase
{
    /// <summary>
    /// Internal logger provided by ASP.Net
    /// </summary>
    private readonly ILogger<DataController> _logger;

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="logger">Logger provided by ASP.Net</param>
    public DataController(ILogger<DataController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Respond to a GET request on all ItemTypes
    /// </summary>
    /// <returns>A list of all itemtypes</returns>
    [HttpGet]
    public async Task<ActionResult<ImageIdsResponse>> Get()
    {
        // ensure that the database is created/connected to properly
        using var db = new DatabaseAccess(this._logger);

        long[] ids = await db.Images.Select(i => i.Id).ToArrayAsync();

        return Ok(new ImageIdsResponse(ids));
    }

    /// <summary>
    /// Respond to a GET request on a specific Image
    /// </summary>
    /// <returns>Details for that Item</returns>
    [HttpGet("id/{id}")]
    public async Task<ActionResult<ImageResponse>> GetImageData(long id)
    {
        using var db = new DatabaseAccess(this._logger);

        DatabaseImage image = await db.Images
            .Where(i => i.Id == id)
            .FirstOrDefaultAsync();

        if (image == null)
        {
            return BadRequest("Image with that id not found");
        }

        return new ImageResponse(
            imageUri: $"image/id/{id}",
            outerHTML: image.OuterHTML,
            imageOcrData: image.ImageOcrData)
        {
            Inference = image.Inference,
            PageSource = image.PageSource,
            PageSummary = image.PageSummary
        };
    }

    /// <summary>
    /// Set the inference for this image
    /// </summary>
    /// <returns>Details for that Item</returns>
    [HttpPatch("id/{id}")]
    public async Task<IActionResult> EditImageData(long id, ImagePatch patch)
    {
        using var db = new DatabaseAccess(this._logger);

        DatabaseImage image = await db.Images
            .Where(i => i.Id == id)
            .FirstOrDefaultAsync();

        if (image == null)
        {
            return BadRequest("Image with that id not found");
        }

        image.Inference = patch.Inference;

        await db.SaveChangesAsync();

        return Ok();
    }

    /// <summary>
    /// Get the actual image data
    /// </summary>
    /// <returns>Details for that Item</returns>
    [HttpGet("formatted/id/{id}")]
    public async Task<ActionResult<string>> GetFormatted(long id)
    {
        using var db = new DatabaseAccess(this._logger);

        DatabaseImage image = await db.Images
            .Where(i => i.Id == id)
            .FirstOrDefaultAsync();

        if (image == null)
        {
            return BadRequest("Image with that id not found");
        }

        string ret = ((Models.IPromptData)image).GetAsString();

        return Ok(ret);
    }

    /// <summary>
    /// Get the actual image data
    /// </summary>
    /// <returns>Details for that Item</returns>
    [HttpGet("image/id/{id}")]
    public async Task<IActionResult> GetImage(long id)
    {
        using var db = new DatabaseAccess(this._logger);

        byte[]? imageData = await db.Images
            .Where(i => i.Id == id)
            .Select(i => i.ImageData)
            .FirstOrDefaultAsync();

        if (imageData == null)
        {
            return BadRequest("Image with that id not found");
        }

        return File(imageData, "image/jpeg");
    }
}
