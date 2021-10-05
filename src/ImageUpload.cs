namespace PigeonAPI;

/// <summary>
/// The image file as uploaded from the web client
/// </summary>
public class ImageUpload
{
    /// <summary>
    /// The X coordinate of the center
    /// </summary>
    /// <value></value>
    public double X { get; set; }

    /// <summary>
    /// The X coordinate of the center
    /// </summary>
    /// <value></value>
    public double Y { get; set; }

    /// <summary>
    /// The Base64 encoded DataUri of the image
    /// </summary>
    /// <value></value>
    public string? ImageUri { get; set; }
}
