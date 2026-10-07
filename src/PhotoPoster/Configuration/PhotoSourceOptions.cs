using System.ComponentModel.DataAnnotations;

namespace PhotoPoster.Configuration;

/// <summary>Where photos come from. Bound from the "PhotoSource" section.</summary>
public sealed class PhotoSourceOptions
{
    public const string SectionName = "PhotoSource";

    /// <summary>Folder to scan for photos.</summary>
    [Required]
    public string RootFolder { get; set; } = string.Empty;

    /// <summary>Whether to include subfolders when scanning.</summary>
    public bool IncludeSubfolders { get; set; } = true;

    /// <summary>File extensions treated as photos (case-insensitive).</summary>
    [MinLength(1)]
    public List<string> Extensions { get; set; } = [];
}
