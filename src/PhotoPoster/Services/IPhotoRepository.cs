using PhotoPoster.Models;

namespace PhotoPoster.Services;

/// <summary>Tracks which photos exist and which have been posted (PP-203).</summary>
public interface IPhotoRepository
{
    /// <summary>Add newly found photos and mark missing ones unavailable.</summary>
    Task SyncAsync(IReadOnlyList<PhotoFile> scannedFiles, CancellationToken cancellationToken);

    /// <summary>Choose the next photo to post on a platform, or null if none are left (PP-204).</summary>
    Task<Photo?> GetNextUnpostedAsync(string platform, CancellationToken cancellationToken);

    Task<PostRecord> AddPostAsync(PostRecord post, CancellationToken cancellationToken);

    Task UpdatePostAsync(PostRecord post, CancellationToken cancellationToken);
}
