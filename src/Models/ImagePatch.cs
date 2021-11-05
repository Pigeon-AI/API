namespace PigeonAPI.Models;

/// <summary>
/// The data sent when patching an image
/// </summary>
public class ImagePatch
{
    /// <summary>
    /// The inference for this image
    /// </summary>
    /// <value></value>
    public string? Inference { get; init; }
}
