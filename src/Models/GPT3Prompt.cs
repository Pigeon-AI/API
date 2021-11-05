using PigeonAPI.Exceptions;

namespace PigeonAPI.Models;

/// <summary>
/// A class representing a prompt to be sent to the GPT3 api, handles all formatting
/// </summary>
public class GPT3Prompt
{
    const string Foreword = "Summarize meaning from HTML and OCR data.";

    /// <summary>
    /// The filled out prompts to use as seeds
    /// </summary>
    /// <returns></returns>
    public List<DatabaseImage> SeedPrompts { get; set; } = new();

    /// <summary>
    /// The new prompt to make an inference for
    /// </summary>
    /// <value></value>
    public PromptData? NewPrompt { get; set;}

    public string BuildPrompt()
    {
        foreach(var prompt in SeedPrompts)
        {
            if (prompt.Inference == null)
            {
                throw new GPTPromptException($"Error: used seed with id {prompt.Id} which didn't have an inference.");
            }
        }

        if (SeedPrompts.Count == 0)
        {
            throw new GPTPromptException("Error: provided no seed prompts to use as an example");
        }

        if (NewPrompt == null)
        {
            throw new GPTPromptException("Error: provided no new prompt to make an inference for.");
        }

        return 
            Foreword + "\n###\n" +
            this.SeedPrompts
            .Select(item => ((IPromptData)item).GetAsString())
            .Aggregate((l, r) => l + r)
            + ((IPromptData)NewPrompt).GetAsString();
    }
}