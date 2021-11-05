namespace PigeonAPI.Models;

/// <summary>
/// Default implementation of the IPromptData interface for when we haven't made an inference yet
/// </summary>
public class PromptData : IPromptData
{
    /// <summary>
    /// The outer html of this data set
    /// </summary>
    /// <value></value>
    public string OuterHTML { get; }

    /// <summary>
    /// The OCR data of this data set
    /// </summary>
    /// <value></value>
    public string ImageOcrData { get; }

    /// <summary>
    /// Main constructor
    /// </summary>
    /// <param name="outerHTML"></param>
    /// <param name="imageOcrData"></param>
    public PromptData(string outerHTML, string imageOcrData)
    {
        this.OuterHTML = outerHTML;
        this.ImageOcrData = imageOcrData;
    }

    /// <summary>
    /// The summary of this dataset if it exists
    /// </summary>
    /// <value>Is always null since this wasn't in the database</value>
    public string? Inference => null;
}