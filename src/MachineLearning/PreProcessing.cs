using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using System.Text.RegularExpressions;
using System.Web;

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
    private static readonly Lazy<string> tempFileDirectory = new(() =>
    {
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
    /// <returns>The file path and the new adjusted center of the element</returns>
    public static async Task<(string, Point)> PreprocessImage(
        Stream imageStream,
        Point center,
        Size elementSize,
        Size windowSize,
        ILogger logger)
    {
        string filePath = Path.Combine(tempFileDirectory.Value, $"{Guid.NewGuid().ToString()}.jpg");

        await using var outStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
        using var image = await Image.LoadAsync(imageStream);

        Size originalSize = image.Size();

        double highDpiScalingFactor = ((double)originalSize.Width) / windowSize.Width;

        int minimumBuffer = Constants.MinimumBuffer;

        // if there is a high dpi scaling factor present, scale all the inputs
        if (Math.Abs(highDpiScalingFactor - 1) > 0.05)
        {
            minimumBuffer = (int)(minimumBuffer * highDpiScalingFactor);
            elementSize = new Size(
                width: (int)(elementSize.Width * highDpiScalingFactor),
                height: (int)(elementSize.Height * highDpiScalingFactor));
            windowSize = new Size(
                width: (int)(windowSize.Width * highDpiScalingFactor),
                height: (int)(windowSize.Height * highDpiScalingFactor));
            center = new Point(
                x: (int)(center.X * highDpiScalingFactor),
                y: (int)(center.Y * highDpiScalingFactor));
        }

        Size newSize = new Size(
            width: elementSize.Width + 2 * minimumBuffer,
            height: elementSize.Height + 2 * minimumBuffer);

        // the new desired starting points on the left and top
        int left = center.X - newSize.Width / 2;
        int top = center.Y - newSize.Height / 2;

        // don't let left or top go below 0
        left = Math.Max(left, 0);
        top = Math.Max(top, 0);

        // don't let the width or height over stretch it
        int width = newSize.Width - Math.Max(newSize.Width + left - originalSize.Width, 0);
        int height = newSize.Height - Math.Max(newSize.Height + top - originalSize.Height, 0);

        // apply the actual image edits in a pipeline
        image.Mutate(i =>
        {
            // crop the image to desired bounds
            i.Crop(new Rectangle(
                x: left, 
                y: top, 
                width: width,
                height: height));
        });
        await image.SaveAsJpegAsync(outStream);

        Point adjustedCenter = new Point(
            x: center.X - left,
            y: center.Y - top
        );

        return (filePath, adjustedCenter);
    }

    /// <summary>
    /// Apply any necessary preprocessing to the html if necessary
    /// </summary>
    /// <param name="html"></param>
    /// <returns></returns>
    public static async Task<string> PreprocessHTML(string html)
    {
        await Task.Run(() => {
            // simplify all special html encoded characters
            html = HttpUtility.HtmlDecode(html)!;

            // remove some unnecessary html data using regex to make it smaller
            html = Regex.Replace(html, @"style=\s*""[^""]+""", "");
            html = Regex.Replace(html, @"class=\s*""[^""]+""", "");
            html = Regex.Replace(html, @"</?span[^>]*>", "");
            html = Regex.Replace(html, @"</?br[^>]*>", "");
            html = Regex.Replace(html, @"\s+", " ");
        });

        return html;
    }
}
