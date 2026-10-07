using System.ComponentModel.DataAnnotations;
using PhotoPoster.Configuration;

namespace PhotoPoster.Tests.Configuration;

// Example tests to copy the shape of. They check the same DataAnnotations
// rules that ValidateDataAnnotations() enforces at startup.
public class ScheduleOptionsTests
{
    [Fact]
    public void Valid_options_pass_validation()
    {
        var options = new ScheduleOptions
        {
            TimesOfDay = [new TimeOnly(9, 0)],
            TimeZoneId = "UTC",
            MaxJitterMinutes = 10,
        };

        Assert.Empty(Validate(options));
    }

    [Fact]
    public void No_post_times_fails_validation()
    {
        var options = new ScheduleOptions { TimesOfDay = [] };

        Assert.Contains(Validate(options), r => r.MemberNames.Contains(nameof(ScheduleOptions.TimesOfDay)));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(121)]
    public void Jitter_out_of_range_fails_validation(int jitter)
    {
        var options = new ScheduleOptions
        {
            TimesOfDay = [new TimeOnly(9, 0)],
            MaxJitterMinutes = jitter,
        };

        Assert.Contains(Validate(options), r => r.MemberNames.Contains(nameof(ScheduleOptions.MaxJitterMinutes)));
    }

    private static List<ValidationResult> Validate(object options)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(options, new ValidationContext(options), results, validateAllProperties: true);
        return results;
    }
}
