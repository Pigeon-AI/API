namespace PigeonAPI.MachineLearning.ExternalProcessing;

using OpenAI_API;
using PigeonAPI.Exceptions;
using PigeonAPI.Models;

/// <summary>
/// Class to handle GPT3 related functioning
/// </summary>
public static class GPT3Inferencing
{
    /// <summary>
    /// Constant with the environment variable key used to get the endpoint secret
    /// </summary>
    const string openAiApiKey = "OPENAI_KEY";

    private static readonly Task<OpenAIAPI> api = Task.Run(() => {
        string apiKey = Environment.GetEnvironmentVariable(openAiApiKey) ??
            throw new EnvironmentVariableException(openAiApiKey);

        Console.WriteLine("Hit task innards");

        return Task.FromResult(new OpenAIAPI(apiKey));
    });

    public static async Task<string> MakeInference(GPT3Prompt prompt)
    {
        var request = new CompletionRequest{
            Prompt = prompt.BuildPrompt(), // the completely structured prompt
            MaxTokens = 128, // max additional tokens in response
            Temperature = 0, // how much randomness, 0 low to 1 high
            PresencePenalty = 0, // how much to penalize already used words
            FrequencyPenalty = 0, // how much to punish repitition
            StopSequence = "\n", // stop sequence to end on
        };

        CompletionResult result = await (await api).Completions.CreateCompletionAsync(request);

        return result.ToString();
    }

    

}
