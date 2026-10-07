namespace PhotoPoster.Models;

/// <summary>A photo the app knows about. Becomes an EF Core entity in PP-201.</summary>
public sealed class Photo
{
    public int Id { get; set; }

    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Hash of the file contents (PP-103), so a renamed or moved file is still
    /// recognised as already posted.
    /// </summary>
    public string ContentHash { get; set; } = string.Empty;

    public DateTimeOffset DiscoveredAt { get; set; }

    /// <summary>False when a scan no longer finds the file on disk.</summary>
    public bool IsAvailable { get; set; } = true;

    public List<PostRecord> Posts { get; set; } = [];
}
