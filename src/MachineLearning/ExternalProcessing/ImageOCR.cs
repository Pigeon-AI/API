namespace PigeonAPI.MachineLearning.ExternalProcessing;

using PigeonAPI.Exceptions;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using SixLabors.ImageSharp;

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
        public string? BoundingBox { get; set; }

        public string? Text { get; set; }
    }

    private struct Line
    {
        public string? BoundingBox { get; set; }

        public Word[]? Words { get; set; }
    }

    private struct Region
    {
        public string? BoundingBox { get; set; }

        public Line[]? Lines { get; set; }
    }

    private class ImageOCRResponse
    {
        public Region[]? Regions { get; set; }
    }

    /// <summary>
    /// Our desired output line format
    /// </summary>
    private struct ProcessedLine
    {
        [JsonPropertyName("centerProximity")]
        public int CenterProximity { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        public ProcessedLine(int centerProximity, string text)
        {
            this.Text = text;
            this.CenterProximity = centerProximity;
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
    public static async Task<string> DoOCR(string imagePath, Point elementCenter, ILogger logger)
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
        var imageStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
        request.Content = new StreamContent(imageStream);

        // tell it it's a jpeg being uploaded
        request.Content.Headers.Add("Content-Type", "image/jpeg");

        // Add the authorization key
        request.Headers.Add("Ocp-Apim-Subscription-Key", ocrApiKey);

        // send the actual request
        var response = await client.SendAsync(request);

        // check that the response is good
        if (response.StatusCode != System.Net.HttpStatusCode.OK) {
            throw new Exception($"Unexpected http response code {response.StatusCode}");
        }

        logger.LogDebug("Received successful response from OCR api.");

        // deserialize the json as an object
        ImageOCRResponse json = await response.Content.ReadFromJsonAsync<ImageOCRResponse>()
            ?? throw new Exception("Error parsing JSON response from OCR");

        // this code right here is kinda a mess nullability wise
        #pragma warning disable CS8604
        #pragma warning disable CS8600
        #pragma warning disable CS8603

        // reformat the ocr output into a simplified form
        ProcessedLine[] reformatted = json.Regions
            .SelectMany(region => region.Lines)
            .Select(line => {
                string box = line.BoundingBox!;

                // reformat the bounding box into a better format
                Regex r = new Regex(@"(\d+),(\d+),(\d+),(\d+)");
                var matches = r.Match(box);

                if (matches == null)
                {
                    throw new FormatException("Bounding box formatted incorrectly");
                }

                // parse the integers from the match
                int left = Int32.Parse(matches.Groups[1].Value);
                int top = Int32.Parse(matches.Groups[2].Value);
                int width = Int32.Parse(matches.Groups[3].Value);
                int height = Int32.Parse(matches.Groups[4].Value);

                // get the center coordinate of this text
                double textCenterX = left + (double)width / 2;
                double textCenterY = top + (double)height / 2;

                // calculate the distance from the center point
                double distance = Math.Sqrt(
                    (textCenterX - elementCenter.X) * (textCenterX - elementCenter.X) +
                    (textCenterY - elementCenter.Y) * (textCenterY - elementCenter.Y));

                string lineText = line.Words
                    .Select(word => word.Text)
                    .Aggregate((s1, s2) => $"{s1} {s2}");

                return new ProcessedLine((int)distance, lineText);
            })
            .ToArray();
        
        #pragma warning restore CS8604
        #pragma warning restore CS8600
        #pragma warning restore CS8603

        // Serialize this back into JSON for api usage
        string result = JsonSerializer.Serialize(reformatted);

        return result;
    }
};
