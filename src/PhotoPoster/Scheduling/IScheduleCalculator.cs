namespace PhotoPoster.Scheduling;

/// <summary>
/// Works out when the next post should happen. Keep this a pure function of
/// "now" plus options so it's easy to unit test (PP-402).
/// </summary>
public interface IScheduleCalculator
{
    DateTimeOffset GetNextRun(DateTimeOffset now);
}
