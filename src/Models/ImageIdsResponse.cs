namespace PigeonAPI.Models;

/// <summary>
/// A response to the main Dump controller relay
/// </summary>
public class ImageIdsResponse
{
    /// <summary>
    /// All Id's of images in the database
    /// </summary>
    /// <value></value>
    public long[] Ids { get; }

    /// <summary>
    /// Main constructor
    /// </summary>
    /// <param name="ids">Id's of images in the database</param>
    public ImageIdsResponse(long[] ids)
    {
        this.Ids = ids;
    }
}
