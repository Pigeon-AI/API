namespace PigeonAPI.MachineLearning.ExternalProcessing;

using PigeonAPI.Exceptions;
using System.Text.Json;

/// <summary>
/// Class for MachineLearning processing functions that call an external process or API
/// </summary>
public static class ImageOCR
{
    /// <summary>
    /// Different structs required for deserializing the OCR output
    /// </summary>
    #region JsonStructs

    private struct Word
    {
        public string BoundingBox { get; set; } = null!;

        public string Text { get; set; } = null!;
    }

    private struct Line
    {
        public string BoundingBox { get; set; } = null!;

        public Word[] Words { get; set; } = null!;
    }

    private struct Region
    {
        public string BoundingBox { get; set; } = null!;

        public Line[] Lines { get; set; } = null!;
    }

    private class ImageOCRResponse
    {
        public Region[] Regions { get; set; } = null!;
    }

    /// <summary>
    /// Our desired output line format
    /// </summary>
    private struct ProcessedLine
    {
        public string BoundingBox { get; set; }

        public string Text { get; set; }

        public ProcessedLine(string boundingBox, string text)
        {
            this.Text = text;
            this.BoundingBox = boundingBox;
        }
    }

    #endregion JsonStructs

    /// <summary>
    /// HttpClient for this class
    /// </summary>
    /// <returns></returns>
    private static readonly HttpClient client = new HttpClient();

    /// <summary>
    /// Make an inference on a preprocessed image
    /// </summary>
    /// <param name="imagePath">The file path of the preprocessed image</param>
    /// <returns>The inference to be returned to the user</returns>
    public static async Task<string> DoOCR(string imagePath)
    {
        const string ocrEndpointKey = "AZURE_OCR_ENDPOINT";
        const string ocrApiKeyKey = "AZURE_OCR_APIKEY";

        // first read in the environment variables with the endpoint info
        string ocrEndpoint = Environment.GetEnvironmentVariable(ocrEndpointKey) ??
            throw new EnvironmentVariableException(ocrEndpointKey);
        string ocrApiKey = Environment.GetEnvironmentVariable(ocrApiKeyKey) ??
            throw new EnvironmentVariableException(ocrApiKeyKey);

        // create response
        var request = new HttpRequestMessage(HttpMethod.Post, ocrEndpoint);

        // Add the image as http content
        byte[] imageBytes = await File.ReadAllBytesAsync(imagePath);
        request.Content = new ByteArrayContent(imageBytes);

        // Add the authorization key
        request.Headers.Add("Ocp-Apim-Subscription-Key", ocrApiKey);

        // send the actual request
        var response = await client.SendAsync(request);

        // check that the response is good
        if (response.StatusCode != System.Net.HttpStatusCode.OK) {
            throw new Exception($"Unexpected http response code {response.StatusCode}");
        }

        // deserialize the json as an object
        ImageOCRResponse json = await response.Content.ReadFromJsonAsync<ImageOCRResponse>()
            ?? throw new Exception("Error parsing JSON response from OCR");

        // reformat the ocr output into a simplified form
        ProcessedLine[] reformatted = json.Regions
            .SelectMany(region => region.Lines)
            .Select(line => {
                string box = line.BoundingBox;
                string lineText = line.Words
                    .Select(word => word.Text)
                    .Aggregate((s1, s2) => $"{s1} {s2}");

                return new ProcessedLine(box, lineText);
            })
            .ToArray();

        // Serialize this back into JSON for api usage
        string result = JsonSerializer.Serialize(reformatted);

        return result;
    }
};
