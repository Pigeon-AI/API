using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace PigeonAPI.MachineLearning;

public static class ImageProcessing
{
    /// <summary>
    /// Make an inference on a preprocessed image
    /// </summary>
    /// <param name="imagePath">The file path of the preprocessed image</param>
    /// <returns>The inference to be returned to the user</returns>
    public static string MakeInference(string imagePath)
    {
        return "This is a sample response.";
    }


    /// <summary>
    /// Preprocess an image saved to disk
    /// </summary>
    /// <param name="imageStream">A stream with the image data</param>
    /// <param name="center">The point we want as center in the new image</param>
    /// <returns></returns>
    public static async Task<string> PreprocessImage(Stream imageStream, Point center, int newWidth, int newHeight) {
        using (var outStream = new MemoryStream())
        using (var image = Image.Load(imageStream))
        {
            await image.SaveAsJpegAsync(outStream);
        }
        
        string filePath = Path.GetTempFileName();

        return filePath;
    }
};
