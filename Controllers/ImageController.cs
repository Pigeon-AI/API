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
    public async Task<ActionResult<string>> Post(ImageUpload upload)
    {
        if (String.IsNullOrEmpty(upload.DataUri) ||
            upload.X < 0 ||
            upload.Y < 0)
        {
            return BadRequest();
        }

        // file path of preprocessed image
        // run in sub function to preserve functional style while kicking large memory variables off the stack
        string filePath = await new Func<Task<string>>(async () => {
            // regex match out the actually binary data from the data uri
            var matchGroups = Regex.Match(upload.DataUri, @"^data:((?<type>[\w\/]+))?;base64,(?<data>.+)$").Groups;
            var base64Data = matchGroups["data"].Value;
            var binData = Convert.FromBase64String(base64Data);

            // preprocess the image and save to disk
            return await MachineLearning.ImageProcessing.PreprocessImage(
                new MemoryStream(binData),
                new SixLabors.ImageSharp.Point(x: upload.X, y: upload.Y),
                300,
                300);
        })();

        string response = MachineLearning.ImageProcessing.MakeInference(filePath);

        return Ok(response);
    }
}
