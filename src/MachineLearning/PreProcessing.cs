using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace PigeonAPI.MachineLearning;

/// <summary>
/// Class for pre processing functions
/// </summary>
public static class PreProcessing
{
    /// <summary>
    /// Randomly generate a temp directory to store images in
    /// </summary>
    /// <returns>The path of this folder</returns>
    private static readonly Lazy<string> tempFileDirectory = new(() => {
        string rootPath = Path.GetTempPath();
        string newDir = Path.Combine(rootPath, Guid.NewGuid().ToString());

        // create that new directory
        Directory.CreateDirectory(newDir);

        return newDir;
    });

    /// <summary>
    /// /// Preprocess an image saved to disk
    /// </summary>
    /// <param name="imageStream">A stream with the image data</param>
    /// <param name="center">The point we want as center in the new image</param>
    /// <returns></returns>
    public static async Task<string> PreprocessImage(
        Stream imageStream,
        Point center,
        int newWidth,
        int newHeight,
        Size windowSize)
    {
        // the new desired starting points on the left and top
        int left = center.X - newWidth / 2;
        int top = center.Y - newHeight / 2;

        string filePath = Path.Combine(tempFileDirectory.Value, $"{Guid.NewGuid().ToString()}.jpg");

        await using (var outStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write))
        using (var image = await Image.LoadAsync(imageStream))
        {
            Size originalSize = image.Size();

            // apply image edits
            image.Mutate(i =>
            {
                // resize if the dimensions are different due to a high dpi screen
                if (originalSize != windowSize) {
                    i.Resize(windowSize);
                }

                // calculate the extra padded needed, 0 if none
                int leftNeededPadding = Math.Max(0, Math.Max(newWidth/2 - center.X, left + newWidth - windowSize.Width));
                int topNeededPadding = Math.Max(0, Math.Max(newHeight/2 - center.Y, top + newHeight - windowSize.Height));

                // add the extra padding if needed
                if (leftNeededPadding > 0 || topNeededPadding > 0)
                {
                    Console.WriteLine("Padding");
                    i.Pad(windowSize.Width + 2 * leftNeededPadding, windowSize.Height + 2 * topNeededPadding)
                    .BackgroundColor(new Rgba32(255, 255, 255));

                    // adjust the top and left coords to account for the shift
                    left += leftNeededPadding;
                    top += topNeededPadding;
                }

                // crop the image to desired bounds
                i.Crop(new Rectangle(x: left, y: top, width: newWidth, height: newHeight));
            });
            await image.SaveAsJpegAsync(outStream);
        }

        return filePath;
    }
}