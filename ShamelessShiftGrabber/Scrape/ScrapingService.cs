using Microsoft.Playwright;
using ShamelessShiftGrabber.Contracts;

namespace ShamelessShiftGrabber.Scrape;

internal class ScrapingService
{
    private readonly ScrapingConfiguration _scrapingConfiguration;
    private readonly ILogger<ScrapingService> _logger;

    public ScrapingService(ScrapingConfiguration scrapingConfiguration, ILogger<ScrapingService> logger)
    {
        _scrapingConfiguration = scrapingConfiguration;
        _logger = logger;
    }

    public async Task<List<ScrapedShift>> ScrapeShifts()
    {
        _logger.LogInformation("= = Scraping starting...");

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

        var shifts = await GetAvailableShifts(page);

        shifts = shifts.DistinctBy(x => x.DetailUrl).ToList();

        _logger.LogInformation($"= = Scraping done, found {shifts.Count} shift(s).");

        return shifts;
    }

    private async Task<List<ScrapedShift>> GetAvailableShifts(IPage page)
    {
        const string condition =
            "tr.MuiTableRow-root:has-text(\"bonus\"), tr.MuiTableRow-root:has-text(\"(1 h\"), " +
            "tr.MuiTableRow-root:has-text(\"(2 h\"), tr.MuiTableRow-root:has-text(\"(3 h\"), " +
            "tr.MuiTableRow-root:has-text(\"(4 h\"), tr.MuiTableRow-root:has-text(\"(5 h\")";
        const string conditionIds =
            "tr.MuiTableRow-root:has-text(\"bonus\") + tr td a, tr.MuiTableRow-root:has-text(\"(1 h\") + tr td a, " +
            "tr.MuiTableRow-root:has-text(\"(2 h\") + tr td a, tr.MuiTableRow-root:has-text(\"(3 h\") + tr td a, " +
            "tr.MuiTableRow-root:has-text(\"(4 h\") + tr td a, tr.MuiTableRow-root:has-text(\"(5 h\") + tr td a";

        var rows = page.Locator(condition);
        var rowsIds = page.Locator(conditionIds);

        var shifts = new List<ScrapedShift>();

        var rowsCount = await rows.CountAsync();
        _logger.LogInformation($"Rows count: {rowsCount}");

        for (var index = 0; index < rowsCount; index++)
        {
            await TryAddShift(rows, index, rowsIds, shifts);
        }

        return shifts;
    }

    private async Task MoveToShiftsOverview(IPage page)
    {
        await page.WaitForURLAsync("https://shameless.sinch.cz/react/dashboard/incoming");
        _logger.LogInformation(page.Url);

        await page.ClickAsync("a[href=\"/react/position\"]");
        await page.WaitForLoadStateAsync();

        await page.WaitForURLAsync("https://shameless.sinch.cz/react/position");
        _logger.LogInformation(page.Url);
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

    private async Task TryAddShift(ILocator rows, int index, ILocator rowsIds, ICollection<ScrapedShift> shifts)
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

            if (unavailableCount < 1)
            {
                var singleShift = new ScrapedShift
                {
                    Name = await nameLoc.InnerTextAsync(),
                    ShiftDate = await dateLoc.InnerTextAsync(),
                    ShiftTime = await timeLoc.InnerTextAsync(),
                    Place = await placeLoc.InnerTextAsync(),
                    Role = await roleLoc.InnerTextAsync(),
                    Occupancy = await occupancyLoc.InnerTextAsync(),
                    DetailUrl = href
                };

                shifts.Add(singleShift);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"***Could not parse row. Error: {ex}");
        }
    }
}