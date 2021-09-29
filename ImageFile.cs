using System.Drawing;

namespace PigeonAPI;

/// <summary>
/// The image file with data, as represented in our database
/// </summary>
public class ImageFile
{
    /// <summary>
    /// The center of the image, where the clicked object is
    /// </summary>
    /// <value></value>
    public Point Center { get; private set; }

    /// <summary>
    /// File path where the image is located
    /// </summary>
    /// <value></value>
    public string? FilePath { get; private set; }

    /// <summary>
    /// Uniquely assigned identifier
    /// </summary>
    /// <value></value>
    public Guid Guid { get; private set; }

    /// <summary>
    /// Default constructor required by entity framework
    /// </summary>
    private ImageFile() {
        this.Guid = Guid.NewGuid();
    }

    /// <summary>
    /// Main constructor used otherwise
    /// </summary>
    public ImageFile(Point center, string filePath) {
        this.Guid = Guid.NewGuid();
        this.Center = center;
        this.FilePath = filePath;
    }
}
