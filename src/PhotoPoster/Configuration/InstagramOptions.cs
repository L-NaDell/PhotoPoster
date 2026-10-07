namespace PhotoPoster.Configuration;

/// <summary>Instagram Graph API settings. Bound from the "Instagram" section.</summary>
public sealed class InstagramOptions
{
    public const string SectionName = "Instagram";

    /// <summary>Numeric Instagram professional account id (PP-501).</summary>
    public string AccountId { get; set; } = string.Empty;

    /// <summary>Long-lived access token. Store in user-secrets.</summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>Graph API base URL. Confirm the current host and version in PP-501.</summary>
    public string BaseUrl { get; set; } = "https://graph.instagram.com/";
}
