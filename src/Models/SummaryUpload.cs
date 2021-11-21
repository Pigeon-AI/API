namespace PigeonAPI.Models;

/// <summary>
/// The image file as uploaded from the web client
/// </summary>
public class SummaryUpload
{
    /// <summary>
    /// The entire html of the webpage
    /// </summary>
    /// <value></value>
    public string? PageSource { get; set; }

    /// <summary>
    /// The entire html of the webpage
    /// </summary>
    /// <value></value>
    public string? PageTitle { get; set; }

    /// <summary>
    /// The entire html of the webpage
    /// </summary>
    /// <value></value>
    public string? PageUrl { get; set; }
}
