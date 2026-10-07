namespace PhotoPoster.Models;

/// <summary>One attempt to post one photo to one platform.</summary>
public sealed class PostRecord
{
    public int Id { get; set; }

    public int PhotoId { get; set; }
    public Photo Photo { get; set; } = null!;

    /// <summary>e.g. "Instagram". Matches <c>ISocialPublisher.Platform</c>.</summary>
    public string Platform { get; set; } = string.Empty;

    public PostStatus Status { get; set; } = PostStatus.Draft;

    public string Caption { get; set; } = string.Empty;

    /// <summary>Hashtags without the leading '#'. Storage format is decided in PP-201.</summary>
    public List<string> Hashtags { get; set; } = [];

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }

    /// <summary>The id the platform returned, for linking back or pulling analytics.</summary>
    public string? ExternalPostId { get; set; }

    public string? Error { get; set; }
}
