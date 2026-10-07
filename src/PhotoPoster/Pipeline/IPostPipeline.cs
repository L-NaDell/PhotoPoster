namespace PhotoPoster.Pipeline;

/// <summary>
/// One full run: scan, sync, pick a photo, caption it, then save a draft or
/// publish it. The Worker calls this once per scheduled slot (PP-403).
/// </summary>
public interface IPostPipeline
{
    Task RunOnceAsync(CancellationToken cancellationToken);
}
