using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.RegularExpressions;
using PigeonAPI.Models;

namespace PigeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class ImageController : ControllerBase
{
    /// <summary>
    /// ASP.Net core required: logger
    /// </summary>
    private readonly ILogger<ImageController> _logger;

    /// <summary>
    /// ASP.Net core required: constructor
    /// </summary>
    /// <param name="logger">Provided logger</param>
    public ImageController(ILogger<ImageController> logger)
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
        if (String.IsNullOrEmpty(upload.ImageUri) ||
            upload.ElementCenterX < 0 ||
            upload.ElementCenterY < 0 ||
            upload.ElementWidth < 0 ||
            upload.ElementHeight < 0 ||
            upload.WindowWidth < 0 ||
            upload.WindowHeight < 0)
        {
            this._logger.LogDebug("Received bad request.");
            return BadRequest("Malformed ImageUpload");
        }

        // file path of preprocessed image
        // run in sub function to preserve functional style while kicking large memory variables off the stack
        string filePath = await new Func<Task<string>>(async () =>
        {
            // regex match out the actually binary data from the data uri
            var matchGroups = Regex.Match(upload.ImageUri, @"^data:((?<type>[\w\/]+))?;base64,(?<data>.+)$").Groups;
            var base64Data = matchGroups["data"].Value;
            var binData = Convert.FromBase64String(base64Data);

            // preprocess the image and save to disk
            return await MachineLearning.PreProcessing.PreprocessImage(
                new MemoryStream(binData),
                new SixLabors.ImageSharp.Point(x: (int)upload.ElementCenterX, y: (int)upload.ElementCenterY),
                new SixLabors.ImageSharp.Size(width: (int)upload.ElementWidth, height: (int)upload.ElementHeight),
                new SixLabors.ImageSharp.Size(width: (int)upload.WindowWidth, height: (int)upload.WindowHeight),
                logger: this._logger);
        })();

        this._logger.LogInformation($"Image processed and written to: {filePath}");

        string response = MachineLearning.ExternalProcessing.MakeInference(filePath);

        this._logger.LogDebug("Image inference complete.");

        

        // Store image and response for future training / use
        using (var db = new DatabaseAccess(this._logger)) {
            byte[] imageData = System.IO.File.ReadAllBytes(filePath);

            await db.Images.AddAsync(new DatabaseImage {
                ImageData = imageData,
                Inference = null,
            });

            await db.SaveChangesAsync();
        }

        return Ok(response);
    }
}
