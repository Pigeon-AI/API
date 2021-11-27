using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.RegularExpressions;
using PigeonAPI.Models;
using SixLabors.ImageSharp;

namespace PigeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class InferenceController : ControllerBase
{
    /// <summary>
    /// ASP.Net core required: logger
    /// </summary>
    private readonly ILogger<InferenceController> _logger;

    /// <summary>
    /// ASP.Net core required: constructor
    /// </summary>
    /// <param name="logger">Provided logger</param>
    public InferenceController(ILogger<InferenceController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// The seed ids to grab from the database for use for inference
    /// </summary>
    /// <value></value>
    private readonly ICollection<long> seedIds = new HashSet<long>{1, 2, 3, 4, 5, 6, 7, 8, 9, 10};

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
            String.IsNullOrEmpty(upload.PageTitle) ||
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

        string outerHTML = await MachineLearning.PreProcessing.StripHTML(upload.OuterHTML);
        //string? pageSource = upload.PageSource == null ? null : await MachineLearning.PreProcessing.PreprocessHTML(upload.PageSource);

        // Get ocr metadata for the image
        string ocrData = await MachineLearning.ExternalProcessing.ImageOCR.DoOCR(fileStream, elementCenter, this._logger);

        this._logger.LogDebug("Image ocr complete.");

        using var db = new DatabaseAccess(this._logger);

        var prompt = new GPT3InferencePrompt
        {
            SeedPrompts = db.Images.Where(item => seedIds.Contains(item.Id)).ToList(),
            NewPrompt = new PromptData(outerHTML: outerHTML, imageOcrData: ocrData, pageTitle: upload.PageTitle)
        };

        this._logger.LogDebug(prompt.BuildPrompt());

        string response = await MachineLearning.ExternalProcessing.GPT3Inferencing.MakeInference(prompt);

        return Ok(response);
    }
}
