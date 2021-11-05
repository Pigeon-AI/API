namespace PigeonAPI.Exceptions;

/// <summary>
/// An exception for a formatting error with the GPT prompt
/// </summary>
public class GPTPromptException : Exception
{
    public GPTPromptException(string message): base(message) {}
}