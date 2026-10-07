using System.ComponentModel.DataAnnotations;

namespace PhotoPoster.Configuration;

/// <summary>LLM caption settings. Bound from the "Caption" section.</summary>
public sealed class CaptionOptions
{
    public const string SectionName = "Caption";

    /// <summary>Model id to call. Decided in PP-301.</summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>API key. Store in user-secrets, never in appsettings.json.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Extra guidance for the prompt: your voice, topics to avoid, etc.</summary>
    public string StyleGuide { get; set; } = string.Empty;

    [Range(0, 30)]
    public int MaxHashtags { get; set; } = 15;
}
