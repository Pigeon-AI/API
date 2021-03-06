namespace PigeonAPI.Models;

/// <summary>
/// A response to the dump controller for a specific image
/// </summary>
public class ImageResponse
{
    /// <summary>
    /// Private constructor needed by entity framework reflection
    /// </summary>
    public ImageResponse(string imageUri, string outerHTML, string imageOcrData, string pageTitle, string pageSummary)
    {
        this.ImageUri = imageUri;
        this.OuterHTML = outerHTML;
        this.ImageOcrData = imageOcrData;
        this.PageTitle = pageTitle;
        this.PageSummary = pageSummary;
    }

    public string ImageUri { get; }

    public string? Inference { get; init; }

    public string OuterHTML { get; }

    public string ImageOcrData { get; }

    public string PageTitle { get; init; }

    public string PageSummary { get; init; }
}
