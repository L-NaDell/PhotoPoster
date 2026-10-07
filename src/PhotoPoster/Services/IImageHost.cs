namespace PhotoPoster.Services;

/// <summary>
/// Makes a local image temporarily reachable at a public URL. Instagram's API
/// fetches images by URL and doesn't accept uploads (PP-503).
/// </summary>
public interface IImageHost
{
    Task<Uri> UploadAsync(string localPath, CancellationToken cancellationToken);

    Task DeleteAsync(Uri hostedUrl, CancellationToken cancellationToken);
}
