using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PigeonAPI.Models;

/// <summary>
/// An individual instance of an item in inventory
/// </summary>
public class DatabaseImage : IPromptData
{
    /// <summary>
    /// The unique identifier for this item
    /// </summary>
    [Key]
    public long Id { get; private set; } = 0;

    /// <summary>
    /// The image we're storing
    /// </summary>
    public byte[] ImageData { get; set; } = null!;

    /// <summary>
    /// The scanned ocr data of this image
    /// </summary>
    public string ImageOcrData { get; set; } = null!;

    /// <summary>
    /// The element.outerHTML field of the selected element
    /// </summary>
    public string OuterHTML { get; set; } = null!;

    /// <summary>
    /// The entire page source of the webpage, might not exist if there are issues
    /// </summary>
    public string? PageSource { get; set; } = null;

    /// <summary>
    /// The Summary of the webpage, might not exist if there are issues
    /// </summary>
    public string? PageSummary { get; set; } = null;

    /// <summary>
    /// Any inference we made on this image
    /// </summary>
    public string? Inference { get; set; } = null;

    /// <summary>
    /// Our main constructor to init non-nullable elements
    /// </summary>
    /// <param name="imageData"></param>
    /// <param name="outerHTML"></param>
    public DatabaseImage(byte[] imageData, string outerHTML, string imageOcrData)
    {
        this.ImageData = imageData;
        this.OuterHTML = outerHTML;
        this.ImageOcrData = imageOcrData;
    }

    /// <summary>
    /// Private constructor needed by entity framework reflection
    /// </summary>
    private DatabaseImage()
    {
    }
}
