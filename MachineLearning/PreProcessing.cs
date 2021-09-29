using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace PigeonAPI.MachineLearning;

/// <summary>
/// Class for pre processing functions
/// </summary>
public static class PreProcessing
{
    /// <summary>
    /// Preprocess an image saved to disk
    /// </summary>
    /// <param name="imageStream">A stream with the image data</param>
    /// <param name="center">The point we want as center in the new image</param>
    /// <returns></returns>
    public static async Task<string> PreprocessImage(Stream imageStream, Point center, int newWidth, int newHeight)
    {
        // the new desired starting points on the left and top
        int left = center.X - newWidth / 2;
        int top = center.Y - newHeight / 2;

        string filePath = Path.GetTempFileName();

        await using (var outStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write))
        using (var image = await Image.LoadAsync(imageStream))
        {
            // apply image edits
            image.Mutate(i =>

                // crop the image to desired bounds
                i.Crop(new Rectangle(x: left, y: top, width: newWidth, height: newHeight)));
            await image.SaveAsJpegAsync(outStream);
        }

        return filePath;
    }
}