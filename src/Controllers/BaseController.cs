using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PigeonAPI.Controllers;

/// <summary>
/// Class to handle all requests on the / route
/// </summary>
[ApiController]
[Route("")]
public class BaseController : ControllerBase
{
    /// <summary>
    /// Internal logger provided by ASP.Net
    /// </summary>
    private readonly ILogger<BaseController> _logger;

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="logger">Logger provided by ASP.Net</param>
    public BaseController(ILogger<BaseController> logger)
    {
        _logger = logger;

        // ensure that the database is created/connected to properly
        using(var db = new DatabaseAccess(logger))
        {
            db.Database.EnsureCreated();
            db.Database.Migrate();
        };
    }

    /// <summary>
    /// Respond to a GET request on all ItemTypes
    /// </summary>
    /// <returns>A list of all itemtypes</returns>
    [HttpGet]
    public ActionResult<string> Get()
    {
        return Ok("Success! Access the api from /swagger/");
    }
}
