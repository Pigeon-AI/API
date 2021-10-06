namespace PigeonAPI.Models;

/// <summary>
/// The image file as uploaded from the web client
/// </summary>
public class ImageUpload
{
    /// <summary>
    /// The X coordinate of the center
    /// </summary>
    /// <value></value>
    public double ElementCenterX { get; set; }

    /// <summary>
    /// The X coordinate of the center
    /// </summary>
    /// <value></value>
    public double ElementCenterY { get; set; }

    /// <summary>
    /// The X coordinate of the center
    /// </summary>
    /// <value></value>
    public double WindowWidth { get; set; }

    /// <summary>
    /// The X coordinate of the center
    /// </summary>
    /// <value></value>
    public double WindowHeight { get; set; }

    /// <summary>
    /// The X coordinate of the center
    /// </summary>
    /// <value></value>
    public double ElementWidth { get; set; }

    /// <summary>
    /// The X coordinate of the center
    /// </summary>
    /// <value></value>
    public double ElementHeight { get; set; }

    /// <summary>
    /// The Base64 encoded DataUri of the image
    /// </summary>
    /// <value></value>
    public string ImageUri { get; set; }
}
