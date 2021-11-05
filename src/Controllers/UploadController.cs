using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.RegularExpressions;
using PigeonAPI.Models;
using SixLabors.ImageSharp;

namespace PigeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UploadController : ControllerBase
{
    /// <summary>
    /// ASP.Net core required: logger
    /// </summary>
    private readonly ILogger<UploadController> _logger;

    /// <summary>
    /// ASP.Net core required: constructor
    /// </summary>
    /// <param name="logger">Provided logger</param>
    public UploadController(ILogger<UploadController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Upload an image and save it for later inferences
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
        (MemoryStream fileStream, Point elementCenter) = await new Func<Task<(MemoryStream, Point)>>(async () =>
        {
            var binData = await MachineLearning.PreProcessing.ConvertBase64ToFile(upload.ImageUri);

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

        this._logger.LogDebug($"Image processed and written to memory.");

        // Get ocr metadata for the image
        string ocrData = await MachineLearning.ExternalProcessing.ImageOCR.DoOCR(fileStream, elementCenter, this._logger);

        this._logger.LogDebug("Image ocr complete.");

        // Store image and response for future training / use
        using (var db = new DatabaseAccess(this._logger))
        {
            // Create stored database item
            var dbItem = new DatabaseImage(
                imageData: fileStream.ToArray(),
                outerHTML: outerHTML,
                imageOcrData: ocrData)
            {
                Inference = null,
                PageSource = pageSource,
                PageSummary = null,
            };

            // add it to the database
            await db.Images.AddAsync(dbItem);

            await db.SaveChangesAsync();
        }

        string response = "This sample was saved to the database.";

        return Ok(response);
    }
}
