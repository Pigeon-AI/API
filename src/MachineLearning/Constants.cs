using SixLabors.ImageSharp;

namespace PigeonAPI.MachineLearning;

/// <summary>
/// Constants relating to the ML processing
/// </summary>
public static class Constants
{
    /// <summary>
    /// The width of all images being processed
    /// </summary>
    public static readonly Size ImageSize = new Size(width: 400, height: 400);

    /// <summary>
    /// The minimum on-screen size buffer that must be between the element and the edge of it's image
    /// </summary>
    public const int MinimumBuffer = 100;
}