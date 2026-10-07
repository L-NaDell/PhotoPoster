using PhotoPoster.Models;

namespace PhotoPoster.Publishers;

/// <summary>
/// One implementation per platform. Instagram first (PP-504). Bluesky, Tumblr
/// and Reddit later test whether this abstraction holds up.
/// </summary>
public interface ISocialPublisher
{
    /// <summary>Stable name stored on <see cref="PostRecord.Platform"/>.</summary>
    string Platform { get; }

    Task<PublishResult> PublishAsync(PublishRequest request, CancellationToken cancellationToken);
}

public sealed record PublishRequest(string ImagePath, GeneratedCaption Caption);

public sealed record PublishResult(bool Succeeded, string? ExternalPostId, string? Error);
