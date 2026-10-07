namespace PhotoPoster.Models;

/// <summary>What the caption generator returns. Hashtags have no leading '#'.</summary>
public sealed record GeneratedCaption(string Caption, IReadOnlyList<string> Hashtags);
