using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;

namespace PigeonAPI.MachineLearning;

/// <summary>
/// Class for pre processing functions
/// </summary>
public static class PreProcessing
{
    /// <summary>
    /// Precompiled regex expression for speed
    /// </summary>
    /// <returns></returns>
    private static readonly Regex whiteSpace = new (@"\s+", RegexOptions.Compiled);

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
    /// <returns>The file as a stream and the new adjusted center of the element</returns>
    public static async Task<(MemoryStream, Point)> PreprocessImage(
        MemoryStream imageStream,
        Point center,
        Size elementSize,
        Size windowSize,
        ILogger logger)
    {
        string filePath = Path.Combine(tempFileDirectory.Value, $"{Guid.NewGuid().ToString()}.jpg");

        var outStream = new MemoryStream();
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

        // reset position to beginning
        outStream.Position = 0;

        return (outStream, adjustedCenter);
    }

    /// <summary>
    /// Gets rid of any extraneous tags and formatting from the html passed in
    /// </summary>
    /// <remarks>
    /// Intended for the element.OuterHTML parsing
    /// </remarks>
    /// <param name="html"></param>
    /// <returns></returns>
    public static async Task<string> StripHTML(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        void RecurseParse(HtmlNode node)
        {
            switch (node.NodeType)
            {
                case HtmlNodeType.Element:

                    // do different actions depending on element type
                    switch (node.Name)
                    {
                        // bad elements we just ignore
                        case "script":
                        case "style":
                        case "meta":
                            node.Remove();
                            break;
                        
                        // good elements we parse and recurse on
                        default:
                            // remove all attributes to elements
                            node.Attributes.RemoveAll();

                            // recursively call this on all children
                            foreach (var child in node.ChildNodes.ToArray())
                            {
                                RecurseParse(child);
                            }

                            // remove this node if empty
                            if (node.ChildNodes.Count == 0)
                            {
                                node.Remove();
                            }
                            break;

                    }
                    break;
                
                case HtmlNodeType.Comment:

                    // remove all comments
                    node.Remove();
                    break;

                case HtmlNodeType.Document:

                    // just recurse for documents
                    foreach (var child in node.ChildNodes.ToArray())
                    {
                        RecurseParse(child);
                    }
                    break;

                case HtmlNodeType.Text:

                    HtmlTextNode textNode = node as HtmlTextNode ?? throw new Exception("Unknown error converting.");

                    // trim first
                    string text = textNode.Text.Trim();

                    // make all whitespace just one space
                    text = whiteSpace.Replace(text, " ");

                    const int maxCount = 200;

                    // shorten if it's too long
                    if (text.Length > maxCount)
                    {
                        text = text.Substring(0, maxCount);
                    }

                    // set it back equal
                    textNode.Text = text;
                
                    // remove if white space
                    if (text.Length == 0)
                    {
                        node.Remove();
                    }
                    break;

            }
        }

        RecurseParse(doc.DocumentNode);

        // create memory stream to write to
        await using MemoryStream ms = new ();

        // write document to memory stream
        doc.Save(ms);

        // reset position
        ms.Position = 0;

        // read as string
        using var sr = new StreamReader(ms);

        return await sr.ReadToEndAsync();
    }

    /// <summary>
    /// Extracts the actual text data and title from the html
    /// </summary>
    /// <remarks>
    /// Intended for the page source parsing
    /// </remarks>
    /// <param name="html"></param>
    /// <returns>(title, data)</returns>
    public static async Task<(string?, string)> ExtractTextFromHtml(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // create memory stream to write to
        await using MemoryStream ms = new ();
        await using StreamWriter sw = new (ms);

        // the title of this webpage, most important
        string? title = null;

        // go through every instance of text in 
        foreach (HtmlNode textNode in doc.DocumentNode.Descendants().Where(node => node.NodeType == HtmlNodeType.Text))
        {
            bool isTitle = false;

            switch (textNode.ParentNode.Name)
            {
                // don't allow undesirable parent tags to murk up the info
                case "script":
                case "style":
                case "meta":
                case "cite":
                    continue;
                
                case "title":
                    isTitle = true;
                    break;
                
                default:
                    break;
            }

            // trim first
            string text = textNode.InnerText.Trim();

            // fix all html funk
            text = HttpUtility.HtmlDecode(text);

            // make all whitespace just one space
            text = whiteSpace.Replace(text, " ");

            // shorten if it's too long
            if (text.Length > Constants.MaxCharactersBetweenTags)
            {
                text = text.Substring(0, Constants.MaxCharactersBetweenTags);
            }
        
            // only write if non empty
            if (text.Length != 0)
            {
                // title is special
                if (isTitle) {
                    title = text;
                }
                else
                {
                    sw.Write(text + " ");
                }
            }
        }

        await sw.FlushAsync();

        // reset position
        ms.Position = 0;

        // read as string
        using var sr = new StreamReader(ms);

        return (title ?? "", await sr.ReadToEndAsync());
    }

    /// <summary>
    /// Function to convert the base64 uri representation of an image ot the actual file
    /// </summary>
    /// <param name="base64Image"></param>
    /// <returns></returns>
    public static async Task<byte[]> ConvertBase64ToFile(string base64Image)
    {
        return await Task.Run(() => {
            // regex match out the actually binary data from the data uri
            var matchGroups = Regex.Match(base64Image, @"^data:((?<type>[\w\/]+))?;base64,(?<data>.+)$").Groups;
            var base64Data = matchGroups["data"].Value;
            var binData = Convert.FromBase64String(base64Data);

            return binData ?? throw new Exception("Unexpected error parsing image uri");
        });
    }
}
