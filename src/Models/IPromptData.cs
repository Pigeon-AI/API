namespace PigeonAPI.Models;

/// <summary>
/// An interface representing one dataset worth of prompt
/// </summary>
public interface IPromptData
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
    /// The summary of this dataset if it exists, includes trailing newline character
    /// </summary>
    /// <value></value>
    public string? Inference { get; } 

    /// <summary>
    /// Default ToString implementation
    /// </summary>
    /// <returns></returns>
    public string GetAsString()
    {
        // inference includes trailing newline if applicable
        return $"High Priority\n{OuterHTML}\nLow Priority\n{ImageOcrData}\nSummary\n{Inference}";
    }
}