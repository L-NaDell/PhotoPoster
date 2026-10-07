using PhotoPoster.Models;

namespace PhotoPoster.Services;

/// <summary>Finds candidate photos. First implementation is the local folder (PP-101).</summary>
public interface IPhotoSource
{
    Task<IReadOnlyList<PhotoFile>> ScanAsync(CancellationToken cancellationToken);
}
