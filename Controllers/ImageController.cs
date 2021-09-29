using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.RegularExpressions;

namespace PigeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Upload an image and get an inference on the image content.
    /// </summary>
    /// <param name="upload">The image being uploaded</param>
    /// <returns>The response from the server</returns>
    [HttpPost]
    public ActionResult<string> Post(ImageUpload upload)
    {
        if (String.IsNullOrEmpty(upload.DataUri) ||
            upload.X < 0 ||
            upload.Y < 0)
        {
            return BadRequest();
        }

        string path = Path.GetTempFileName();

        {
            var matchGroups = Regex.Match(upload.DataUri, @"^data:((?<type>[\w\/]+))?;base64,(?<data>.+)$").Groups;
            var base64Data = matchGroups["data"].Value;
            var binData = Convert.FromBase64String(base64Data);
            System.IO.File.WriteAllBytes(path, binData);
        }

        var image = new ImageFile(
            filePath: path,
            center: new System.Drawing.Point(x: upload.X, y: upload.Y)
        );

        using (var db = new DatabaseAccess())
        {
            db.Images!.Add(image);
        }

        return Ok("This is a sample response.");
    }
}
