namespace PigeonAPI.Models;

/// <summary>
/// A response to the dump controller for a specific image
/// </summary>
public class ImageResponse
{
    /// <summary>
    /// Private constructor needed by entity framework reflection
    /// </summary>
    public ImageResponse(string imageUri, string outerHTML)
    {
        this.ImageUri = imageUri;
        this.OuterHTML = outerHTML;
    }

    public string ImageUri { get; }

    public string? Inference { get; init; }

    public string OuterHTML { get; }

    public string? PageSource { get; init; }
}
