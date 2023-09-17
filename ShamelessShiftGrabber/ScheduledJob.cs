﻿using Quartz;
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
        _logger.LogDebug("__________________________________________________________________________");
        _logger.LogInformation("= = = Starting scheduled job");

        var availableShifts = await _scrapingService.ScrapeShifts();

        // Note: for debugging purposes:
        //var availableShifts = new List<ScrapedShift>
        //{
        //    new ScrapedShift
        //    {
        //        Name = "pouze test změna",
        //        DetailUrl = "/react/position/1",
        //        ShiftDate = "30. 8. 2023",
        //        ShiftTime = "5:00 PM - 10:00 PM",
        //        Place = "blablab",
        //        Role = "blablab",
        //        Occupancy = "0/1"
                
        //    }
        //};

        if (availableShifts.Count <= 0)
        {
            LogFinishedJob();
            return;
        }

        var filteredShifts = await _shiftRepository.Filter(availableShifts);

        if (filteredShifts.Count == 0)
        {
            _logger.LogDebug("No shifts to be sent to macrodroid");
            LogFinishedJob();
            return;
        }

        var macrodroidResult = await _macrodroid.Send(filteredShifts);

        if (macrodroidResult)
        {
            _logger.LogInformation($"Successfully sent {filteredShifts.Count} shifts to macrodroid");
        }

        LogFinishedJob();
    }

    private void LogFinishedJob() => _logger.LogInformation("= = = Finished scheduled job");
}