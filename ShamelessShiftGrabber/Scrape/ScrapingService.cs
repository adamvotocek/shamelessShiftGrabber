using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Validations;
using Microsoft.Playwright;
using ShamelessShiftGrabber.GoogleSheets;

namespace ShamelessShiftGrabber.Scrape;

internal class ScrapingService
{
    private readonly ScrapingConfiguration _scrapingConfiguration;
    private readonly GoogleSheetsConditionService _googleSheetsConditionService;
    private readonly ILogger<ScrapingService> _logger;
    private readonly AppInsightsService _appInsightsService;

    public ScrapingService(
        ScrapingConfiguration scrapingConfiguration,
        GoogleSheetsConditionService googleSheetsConditionService,
        ILogger<ScrapingService> logger, 
        AppInsightsService appInsightsService
    )
    {
        _scrapingConfiguration = scrapingConfiguration;
        _googleSheetsConditionService = googleSheetsConditionService;
        _logger = logger;
        _appInsightsService = appInsightsService;
    }

    public async Task<List<ScrapedShift>> ScrapeShifts()
    {
        _logger.LogDebug("= = Scraping starting...");

        var sheetInformation = _googleSheetsConditionService.GetSheetInformation();

        if (sheetInformation == null)
        {
            const string message = "Failed to get Google sheets data";
            _logger.LogError(message);
            _appInsightsService.TrackError(message);

            return new List<ScrapedShift>();
        }   

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(
            new BrowserTypeLaunchOptions
            {
                Headless = _scrapingConfiguration.IsHeadless,
                Timeout = _scrapingConfiguration.Timeout,
                SlowMo = _scrapingConfiguration.SlowMo
            }
        );

        var page = await browser.NewPageAsync();

        await Login(page);

        await MoveToShiftsOverview(page);

        await page.WaitForTimeoutAsync(5000);

        var shifts = await GetAvailableShifts(page, sheetInformation);
        _logger.LogDebug($"Scraped {shifts.Count} shift(s).");

        shifts = shifts.DistinctBy(x => x.DetailUrl).ToList();

        _logger.LogDebug($"After duplicates removed: {shifts.Count} shift(s).");
        _logger.LogInformation($"Returning {shifts.Count} shift(s) from scraping.");
        _logger.LogDebug("= = Scraping done.");

        return shifts;
    }

    private async Task<List<ScrapedShift>> GetAvailableShifts(IPage page, SheetInformation sheetInformation)
    {
        var rows = page.Locator(sheetInformation.Condition);
        var rowsIds = page.Locator(sheetInformation.ConditionIds);

        var shifts = new List<ScrapedShift>();

        var rowsCount = await rows.CountAsync();
        _logger.LogDebug($"Rows found using css conditions count: {rowsCount}");

        for (var index = 0; index < rowsCount; index++)
        {
            await TryAddShift(rows, index, rowsIds, shifts, sheetInformation.AvailableDates);
        }

        return shifts;
    }

    private async Task MoveToShiftsOverview(IPage page)
    {
        await page.WaitForURLAsync("https://shameless.sinch.cz/react/dashboard/incoming");
        _logger.LogDebug(page.Url);

        await page.ClickAsync("a[href=\"/react/position\"]");
        await page.WaitForLoadStateAsync();

        await page.WaitForURLAsync("https://shameless.sinch.cz/react/position");
        _logger.LogDebug(page.Url);
        await page.WaitForLoadStateAsync();
    }

    private async Task Login(IPage page)
    {
        await page.GotoAsync("https://shameless.sinch.cz/");

        await page.TypeAsync("#UserEmail", _scrapingConfiguration.UserName);
        await page.TypeAsync("#UserPassword", _scrapingConfiguration.Password);
        await page.ClickAsync("#UserLoginForm input[type=submit]");
        await page.WaitForLoadStateAsync();
    }

    private async Task TryAddShift(
        ILocator rows, 
        int index, 
        ILocator rowsIds, 
        ICollection<ScrapedShift> shifts,
        List<DateTime> availableDates
    )
    {
        var row = rows.Nth(index);
        var rowId = rowsIds.Nth(index);

        try
        {
            var href = await rowId.GetAttributeAsync("href");

            var cells = row.Locator("td");

            var nameLoc = cells.Nth(0);
            var dateLoc = cells.Nth(1);
            var timeLoc = cells.Nth(2);
            var placeLoc = cells.Nth(3);
            var roleLoc = cells.Nth(4);
            var occupancyLoc = cells.Nth(5);

            var unavailableLoc = cells.Nth(6).Locator("svg[title*=\"Nespl\"]");
            var unavailableCount = await unavailableLoc.CountAsync();

            var isUnavailable = unavailableCount > 0;

            if (isUnavailable)
            {
                _logger.LogDebug($"Scraped shift is unavailable: {href}");
            }
            else
            {

                var singleShift = new ScrapedShift
                {
                    Name = await nameLoc.InnerTextAsync(),
                    ShiftTime = await timeLoc.InnerTextAsync(),
                    Place = await placeLoc.InnerTextAsync(),
                    Role = await roleLoc.InnerTextAsync(),
                    Occupancy = await occupancyLoc.InnerTextAsync(),
                    DetailUrl = href
                };

                var scrapedDate = await dateLoc.InnerTextAsync();
                singleShift.TryFillShiftDate(scrapedDate);

                AddScrapedShift(shifts, availableDates, singleShift);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"***Could not parse row. Error: {ex}");
        }
    }

    private void AddScrapedShift(ICollection<ScrapedShift> shifts, List<DateTime> availableDates, ScrapedShift singleShift)
    {
        if (!availableDates.Any())
        {
            _logger.LogDebug($"Adding scraped shift: {singleShift.ShiftDate}");
 
            shifts.Add(singleShift);
            return;
        }

        var isShiftDateInAvailableDates = availableDates.Contains(singleShift.ShiftDate);
        if (isShiftDateInAvailableDates)
        {
            _logger.LogDebug($"Shift date is in available dates: {singleShift.ShiftDate}. Adding scraped shift.");
            shifts.Add(singleShift);

            return;
        }

        _logger.LogDebug($"Shift date is not in available dates: {singleShift.ShiftDate}. Skipping shift.");
    }
}