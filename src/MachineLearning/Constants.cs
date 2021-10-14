using SixLabors.ImageSharp;

namespace PigeonAPI.MachineLearning;

/// <summary>
/// Constants relating to the ML processing
/// </summary>
public static class Constants
{
    /// <summary>
    /// The minimum on-screen size buffer that must be between the element and the edge of it's image
    /// </summary>
    public const int MinimumBuffer = 100;

    /// <summary>
    /// Max characters between tags when processing HTML
    /// </summary>
    public const int MaxCharactersBetweenTags = 250;

    /// <summary>
    /// Max number of OCR lines to be included in the output
    /// </summary>
    public const int MaxLinesOcr = 10;
}