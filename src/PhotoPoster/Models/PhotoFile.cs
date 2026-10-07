namespace PhotoPoster.Models;

/// <summary>A photo found on disk by a scan. Not persisted. See <see cref="Photo"/>.</summary>
public sealed record PhotoFile(string FullPath, long SizeBytes, DateTimeOffset LastModifiedUtc);
