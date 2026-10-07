using System.ComponentModel.DataAnnotations;

namespace PhotoPoster.Configuration;

/// <summary>When to post. Bound from the "Schedule" section.</summary>
public sealed class ScheduleOptions
{
    public const string SectionName = "Schedule";

    /// <summary>Local times of day to post, e.g. "09:00", "18:30".</summary>
    [MinLength(1)]
    public List<TimeOnly> TimesOfDay { get; set; } = [];

    /// <summary>Windows or IANA time zone id the times above are in.</summary>
    [Required]
    public string TimeZoneId { get; set; } = "UTC";

    /// <summary>
    /// Random offset (+/-) applied to each slot so posts don't land at the exact
    /// same second every day, which can look bot-like.
    /// </summary>
    [Range(0, 120)]
    public int MaxJitterMinutes { get; set; } = 20;
}
