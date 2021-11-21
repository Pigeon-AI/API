namespace PigeonAPI.MachineLearning.ExternalProcessing;

using OpenAI_API;
using PigeonAPI.Exceptions;
using PigeonAPI.Models;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

/// <summary>
/// Class to handle GPT3 related functioning
/// </summary>
public static class GPT3Inferencing
{
    /// <summary>
    /// Constant with the environment variable key used to get the endpoint secret
    /// </summary>
    const string openAiApiKeyVariableName = "OPENAI_KEY";

    /// <summary>
    /// Endpoint for the davinci engine
    /// </summary>
    const string davinciEndpoint = @"https://api.openai.com/v1/engines/davinci/completions";

    /// <summary>
    /// Endpoint for the davinci instruct beta engine
    /// </summary>
    const string davinciInstructEndpoint = @"https://api.openai.com/v1/engines/davinci-instruct-beta/completions";

    /// <summary>
    /// HttpClient for this class
    /// </summary>
    /// <returns></returns>
    private static readonly HttpClient client = new HttpClient();

    /// <summary>
    /// The string with the OpenAI api authorization key
    /// </summary>
    /// <returns></returns>
    private static readonly Task<string> openAiApiKey = Task.Run(() => 
        Environment.GetEnvironmentVariable(openAiApiKeyVariableName) ??
        throw new EnvironmentVariableException(openAiApiKeyVariableName));

    /// <summary>
    /// Helper function to handle all the ugliness involved in calling the OpenAI api
    /// </summary>
    /// <param name="prompt">The prompt string</param>
    /// <param name="stopSequences">The array of stop sequences</param>
    /// <param name="max_tokens">The max number of tokens in the response</param>
    /// <param name="temperature">The randomness amount in the response, 0 low 1 high</param>
    private static async Task<string> callGptApi(string endpoint, string prompt, string[] stopSequences, int max_tokens = 128, double temperature = 0)
    {
        // create request
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        
        // Add the json data via a serialized anonymous class
        request.Content = JsonContent.Create(new {
            prompt = prompt, // the completely structured prompt
            max_tokens = max_tokens, // max additional tokens in response
            temperature = temperature, // how much randomness, 0 low to 1 high
            stop = stopSequences, // stop sequence to end on
        });

        // Add the authorization key
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await openAiApiKey);
    
        // send the actual request
        var response = await client.SendAsync(request);

        // check that the response is good
        if (response.StatusCode != System.Net.HttpStatusCode.OK) {
            throw new Exception($"Unexpected http response code {response.StatusCode}");
        }

        var stream = await response.Content.ReadAsStreamAsync();
        JsonDocument json = await JsonDocument.ParseAsync(stream);

        // get obj.choices[0].text
        return json.RootElement.GetProperty("choices").EnumerateArray().First().GetProperty("text").GetString()!;
    }


    /// <summary>
    /// Helper function to handle all the ugliness involved in calling the OpenAI api
    /// </summary>
    /// <param name="prompt">The prompt string</param>
    /// <param name="stopSequence">The single stop sequences</param>
    /// <param name="max_tokens">The max number of tokens in the response</param>
    /// <param name="temperature">The randomness amount in the response, 0 low 1 high</param>
    private static async Task<string> callGptApi(string endpoint, string prompt, string stopSequence, int max_tokens = 128, double temperature = 0)
    {
        return await callGptApi(
            endpoint: endpoint,
            prompt: prompt,
            stopSequences: new string[]{stopSequence}, 
            max_tokens: max_tokens, 
            temperature: temperature);
    }

    /// <summary>
    /// Make an inference given a prompt
    /// </summary>
    /// <param name="prompt"></param>
    /// <returns></returns>
    public static async Task<string> MakeInference(GPT3InferencePrompt prompt)
    {
        return await callGptApi(davinciEndpoint, prompt.BuildPrompt(), "\n");
    }

    /// <summary>
    /// Summarize a page given the pageTitle and pageText
    /// </summary>
    /// <param name="pageTitle"></param>
    /// <param name="pageText"></param>
    /// <returns></returns>
    public static async Task<string> SummarizePage(string? pageTitle, string pageText)
    {
        StringBuilder prompt = new();
        prompt.Append("Given a website title and some text from the website, provide a one sentence summary\n\n");

        if (pageTitle != null)
        {
            prompt.Append($"Title:\n{pageTitle}\n");
        }
        prompt.Append($"Text:\n{pageText}\n");

        prompt.Append("Summary:\n");
        
        return await callGptApi(davinciInstructEndpoint, prompt.ToString(), "\"\"\"");
    }

    /// <summary>
    /// Summarize a page given the pageTitle and pageText
    /// </summary>
    /// <param name="pageTitle"></param>
    /// <param name="pageText"></param>
    /// <returns></returns>
    public static async Task<string> SummarizeNytCnn(string? seed, string data)
    {
        StringBuilder prompt = new(seed);

        prompt.Append($"Data:\n{data}\n");

        prompt.Append("Summary:\n");
        
        return await callGptApi(davinciEndpoint, prompt.ToString(), "\"\"\"");
    }
}
