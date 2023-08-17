using Quartz;
using ShamelessShiftGrabber.Repository;
using ShamelessShiftGrabber.Scrape;

namespace ShamelessShiftGrabber;

internal class ScheduledJob : IJob
{
    private readonly ILogger<ScheduledJob> _logger;
    private readonly ScrapingService _scrapingService;

    private readonly ShiftRepository _shiftRepository;
    private readonly Macrodroid.Macrodroid _macrodroid;

    public ScheduledJob(
        ILogger<ScheduledJob> logger,
        ScrapingService scrapingService, ShiftRepository shiftRepository, Macrodroid.Macrodroid macrodroid)
    {
        _logger = logger;
        _scrapingService = scrapingService;
        _shiftRepository = shiftRepository;
        _macrodroid = macrodroid;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("__________________________________________________________________________");
        _logger.LogInformation("= = = Starting scheduled job");

        var availableShifts = await _scrapingService.ScrapeShifts();

        _logger.LogInformation($"Received shifts: {availableShifts.Count}");

        if (availableShifts.Count <= 0)
        {
            LogFinishedJob();
            return;
        }

        var filteredShifts = await _shiftRepository.Filter(availableShifts);

        if (filteredShifts.Count == 0)
        {
            _logger.LogInformation("No shifts found to send to macrodroid");
            LogFinishedJob();
            return;
        }

        var macrodroidResult = await _macrodroid.Send(filteredShifts);

        _logger.LogInformation("Macrodroid finished with: " + (macrodroidResult ? "success" : "failure"));
        LogFinishedJob();
    }

    private void LogFinishedJob() => _logger.LogInformation("= = = Finished scheduled job");
}