using PhotoPoster.Models;

namespace PhotoPoster.Services;

/// <summary>Turns an image into a caption and hashtags using an LLM vision API (PP-303).</summary>
public interface ICaptionGenerator
{
    Task<GeneratedCaption> GenerateAsync(string imagePath, CancellationToken cancellationToken);
}
