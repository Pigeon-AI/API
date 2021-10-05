using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.RegularExpressions;

namespace PigeonAPI.Controllers;

[ApiController]
[Route("/")]
public class BaseController : ControllerBase
{
    /// <summary>
    /// ASP.Net core required: logger
    /// </summary>
    private readonly ILogger<ImageController> _logger;

    /// <summary>
    /// ASP.Net core required: constructor
    /// </summary>
    /// <param name="logger">Provided logger</param>
    public BaseController(ILogger<ImageController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Base get request to see service is up
    /// </summary>
    /// <returns>Success</returns>
    [HttpPost]
    public IActionResult Get()
    {
        return Ok("Success!");
    }
}
