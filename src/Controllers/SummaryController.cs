using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.RegularExpressions;
using PigeonAPI.Models;
using SixLabors.ImageSharp;

namespace PigeonAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class SummaryController : ControllerBase
{
    /// <summary>
    /// ASP.Net core required: logger
    /// </summary>
    private readonly ILogger<SummaryController> _logger;

    /// <summary>
    /// ASP.Net core required: constructor
    /// </summary>
    /// <param name="logger">Provided logger</param>
    public SummaryController(ILogger<SummaryController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Upload an image and get an inference on the image content.
    /// </summary>
    /// <param name="upload">The image being uploaded</param>
    /// <returns>The response from the server</returns>
    [HttpPost]
    public async Task<ActionResult<string>> Post(SummaryUpload upload)
    {
        if (String.IsNullOrEmpty(upload.PageSource) ||
            String.IsNullOrEmpty(upload.PageTitle) ||
            String.IsNullOrEmpty(upload.PageUrl))
        {
            this._logger.LogDebug("Received bad request.");
            return BadRequest("Malformed SummaryUpload");
        }

        // if (upload.PageUrl.Contains("nytimes.com"))
        // {
        //     string text = await MachineLearning.PreProcessing.ExtractNYTData(upload.PageSource);

        //     // send it to GPT3 to do preliminary summarization
        //     Task<string> summary = MachineLearning.ExternalProcessing.GPT3Inferencing.SummarizeNYTPage(
        //         html: text
        //     );
        // }
        // else if (upload.PageUrl.Contains("cnn.com"))
        // {
        //     string text = await MachineLearning.PreProcessing.ExtractCNNData(upload.PageSource);
        // }
        // else
        // {
        //     return BadRequest("Summarizing only works for nytimes and cnn at the moment.");
        // }
        string pageText = await MachineLearning.PreProcessing.ExtractTextFromHtml(upload.PageSource);
        string pageTitle = upload.PageTitle;

        // send it to GPT3 to do preliminary summarization
        string summary = await MachineLearning.ExternalProcessing.GPT3Inferencing.SummarizePage(
            pageTitle: pageTitle,
            pageText: pageText
        );

        return Ok(summary);
    }
}
