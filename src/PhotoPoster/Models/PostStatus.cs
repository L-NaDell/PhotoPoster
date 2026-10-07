namespace PhotoPoster.Models;

public enum PostStatus
{
    /// <summary>Caption generated, waiting for approval (PP-603).</summary>
    Draft,
    Approved,
    Rejected,
    Published,
    Failed,
}
