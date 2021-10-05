using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PigeonAPI.Models;

/// <summary>
/// An individual instance of an item in inventory
/// </summary>
public class DatabaseImage
{
    /// <summary>
    /// The unique identifier for this item
    /// </summary>
    [Key]
    public long Id { get; set; } = 0;

    /// <summary>
    /// The image we're storing
    /// </summary>
    public byte[]? ImageData { get; set; } = null;

    /// <summary>
    /// Any inference we made on this image
    /// </summary>
    public string? Inference { get; set; } = null;
}
