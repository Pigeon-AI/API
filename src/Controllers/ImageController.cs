using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.RegularExpressions;
using PigeonAPI.Models;
using SixLabors.ImageSharp;

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
            String.IsNullOrEmpty(upload.OuterHTML) ||
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
        (string filePath, Point elementCenter) = await new Func<Task<(string, Point)>>(async () =>
        {
            // regex match out the actually binary data from the data uri
            var matchGroups = Regex.Match(upload.ImageUri, @"^data:((?<type>[\w\/]+))?;base64,(?<data>.+)$").Groups;
            var base64Data = matchGroups["data"].Value;
            var binData = Convert.FromBase64String(base64Data);

            // preprocess the image and save to disk
            return await MachineLearning.PreProcessing.PreprocessImage(
                new MemoryStream(binData),
                new Point(x: (int)upload.ElementCenterX, y: (int)upload.ElementCenterY),
                new Size(width: (int)upload.ElementWidth, height: (int)upload.ElementHeight),
                new Size(width: (int)upload.WindowWidth, height: (int)upload.WindowHeight),
                logger: this._logger);
        })();

        string outerHTML = await MachineLearning.PreProcessing.PreprocessHTML(upload.OuterHTML);
        string? pageSource = upload.PageSource == null ? null : await MachineLearning.PreProcessing.PreprocessHTML(upload.PageSource);

        this._logger.LogInformation($"Image processed and written to: {filePath}");

        // Get ocr metadata for the image
        string ocrData = await MachineLearning.ExternalProcessing.ImageOCR.DoOCR(filePath, elementCenter, this._logger);

        this._logger.LogDebug("Image ocr complete.");

        // Store image and response for future training / use
        using (var db = new DatabaseAccess(this._logger))
        {
            // read the image into memory to be stored persistently in the database
            byte[] imageData = System.IO.File.ReadAllBytes(filePath);

            // Create stored database item
            var dbItem = new DatabaseImage(
                imageData: imageData,
                outerHTML: outerHTML,
                imageOcrData: ocrData)
            {
                Inference = null,
                PageSource = pageSource,
            };

            // add it to the database
            await db.Images.AddAsync(dbItem);

            await db.SaveChangesAsync();
        }

        string response = "This is a sample response.";

        return Ok(response);
    }
}
