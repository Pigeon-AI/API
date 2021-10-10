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
    /// <returns></returns>
    public static async Task<string> PreprocessImage(
        Stream imageStream,
        Point center,
        Size elementSize,
        Size windowSize,
        ILogger logger)
    {
        // start by assuming the element will fit and no resizing will be
        // necessary at the end
        Size newSize = Constants.ImageSize;
        bool needsEndResizing = false;

        // determine whether the element is too large and we must crop
        // to a wider bounds and then shrink down later
        if (elementSize.Width + 2 * Constants.MinimumBuffer > Constants.ImageSize.Width ||
            elementSize.Height + 2 * Constants.MinimumBuffer > Constants.ImageSize.Height)
        {
            logger.LogDebug("Element is too large, applying alternate bounds");

            // mark bool for later that it'll need to get shrunk a bit extra at the end
            needsEndResizing = true;
            
            // must figure out whether it's "more wide" or "more tall" based on aspect ratio
            double existingAspectRatio = ((double)elementSize.Width) / elementSize.Height;
            double desiredAspectRatio = ((double)Constants.ImageSize.Width) / Constants.ImageSize.Height;

            // element is "too wide"
            // Make the new width the element width plus the padding on either side
            // size the new height according to the aspect ratio
            if (existingAspectRatio > desiredAspectRatio)
            {
                newSize = new Size {
                    Width = elementSize.Width + 2 * Constants.MinimumBuffer
                };
                newSize.Height = (int)(newSize.Width / desiredAspectRatio);

                // double check that this calculation was done correctly
                // and the new height is big enough to contain the element
                if (newSize.Height < elementSize.Height + 2 * Constants.MinimumBuffer) {
                    throw new Exception("Should never happen, code written wrong.");
                }
            }
            else 

            // other way around, make the new height the element height plus the padding
            // size the width according to the aspect ratio
            {
                newSize = new Size {
                    Height = elementSize.Height + 2 * Constants.MinimumBuffer
                };
                newSize.Width = (int)(newSize.Height * desiredAspectRatio);

                // double check that this calculation was done correctly
                // and the new height is big enough to contain the element
                if (newSize.Width < elementSize.Width + 2 * Constants.MinimumBuffer) {
                    throw new Exception("Should never happen, code written wrong.");
                }
            }
        }

        /*
            at this point in the code we now know what size we want to crop and pad too
            we know that size will be a proper aspect ratio of the final desired size
            and we also know whether that size is equal to the final size
            if it's not we'll have to shrink it down at the end
        */

        // the new desired starting points on the left and top
        int left = center.X - newSize.Width / 2;
        int top = center.Y - newSize.Height / 2;

        string filePath = Path.Combine(tempFileDirectory.Value, $"{Guid.NewGuid().ToString()}.jpg");

        await using (var outStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write))
        using (var image = await Image.LoadAsync(imageStream).ConfigureAwait(false))
        {
            Size originalSize = image.Size();

            // apply the actual image edits in a pipeline
            image.Mutate(i =>
            {
                // resize if the dimensions are different due to a high dpi screen
                // this must be done first otherwise the calculations get nuts
                if (originalSize != windowSize)
                {
                    i.Resize(windowSize);
                }

                // calculate the extra padded needed, 0 if none
                int leftNeededPadding = Math.Max(0, Math.Max(newSize.Width / 2 - center.X, left + newSize.Width - windowSize.Width));
                int topNeededPadding = Math.Max(0, Math.Max(newSize.Height / 2 - center.Y, top + newSize.Height - windowSize.Height));

                // add the extra padding if needed
                if (leftNeededPadding > 0 || topNeededPadding > 0)
                {
                    logger.LogDebug("Padding the image");
                    i.Pad(windowSize.Width + 2 * leftNeededPadding, windowSize.Height + 2 * topNeededPadding)
                    .BackgroundColor(new Rgba32(255, 255, 255));

                    // adjust the top and left coords to account for the shift
                    left += leftNeededPadding;
                    top += topNeededPadding;
                }

                // crop the image to desired bounds
                i.Crop(new Rectangle(x: left, y: top, newSize.Width, newSize.Height));

                // finally, if our element was too big early and the current 
                // size must be shrunk down, do it here
                if (needsEndResizing)
                {
                    i.Resize(Constants.ImageSize);
                }
            });
            await image.SaveAsJpegAsync(outStream).ConfigureAwait(false);
        }

        return filePath;
    }

    /// <summary>
    /// Apply any necessary preprocessing to the html if necessary
    /// </summary>
    /// <param name="html"></param>
    /// <returns></returns>
    public static async Task<string> PreprocessHTML(string html)
    {
        await Task.Run(() => {});

        return html;
    }
}
