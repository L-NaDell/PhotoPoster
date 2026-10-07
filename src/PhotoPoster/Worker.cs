using Microsoft.Extensions.Options;
using PhotoPoster.Configuration;

namespace PhotoPoster;

/// <summary>
/// The long-running background loop. Its only job is "wait until the next
/// scheduled slot, then run the pipeline once". Keep real logic out of here.
/// Put it in IPostPipeline so it can be tested without a running host.
/// </summary>
public sealed class Worker(
    ILogger<Worker> logger,
    IOptionsMonitor<ScheduleOptions> schedule,
    TimeProvider time) : BackgroundService
{
    // TODO(PP-402): replace with IScheduleCalculator.GetNextRun(now).
    private static readonly TimeSpan PlaceholderInterval = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "PhotoPoster started. Post times: {Times} ({TimeZone})",
            string.Join(", ", schedule.CurrentValue.TimesOfDay),
            schedule.CurrentValue.TimeZoneId);

        while (!stoppingToken.IsCancellationRequested)
        {
            // TODO(PP-403): create a DI scope, resolve IPostPipeline, call RunOnceAsync.
            // Catch and log exceptions here so one bad run doesn't kill the worker.
            logger.LogInformation(
                "Tick at {Now:u}. Pipeline not implemented yet, see docs/BACKLOG.md.",
                time.GetLocalNow());

            await Task.Delay(PlaceholderInterval, time, stoppingToken);
        }
    }
}
