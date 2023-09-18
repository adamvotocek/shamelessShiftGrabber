using Microsoft.EntityFrameworkCore;
using ShamelessShiftGrabber.Scrape;

namespace ShamelessShiftGrabber.Repository;

internal class ShiftRepository
{
    private readonly ILogger<ShiftRepository> _logger;
    private readonly ShiftsDatabaseContext _shiftsDatabaseContext;

    public ShiftRepository(
        ILogger<ShiftRepository> logger,
        ShiftsDatabaseContext shiftsDatabaseContext
    )
    {
        _logger = logger;
        _shiftsDatabaseContext = shiftsDatabaseContext;
    }

    public async Task ReadAndLogAllTheRowsFromTheShiftsTable()
    {
        _logger.LogDebug(".......Reading all the rows from the shifts table......");

        await _shiftsDatabaseContext.Shifts
            .AsNoTracking()
            .ForEachAsync(s => _logger.LogDebug($"Shift: {s.Id} - {s.Name}"));
        _logger.LogDebug(".......................................................");
    }

    public async Task<ICollection<ScrapedShift>> Filter(List<ScrapedShift> shifts)
    {
        _logger.LogDebug($"-- Before DB filtering: {shifts.Count} shifts");

        var filteredShifts = new List<ScrapedShift>();

        var existingShifts = await _shiftsDatabaseContext.Shifts
            .Where(s => s.ShiftDate > DateTime.Now)
            .ToListAsync();

        foreach (var incomingShift in shifts)
        {
            await ProcessShift(incomingShift, existingShifts, filteredShifts);
        }

        if (filteredShifts.Count > 0)
        {
            _logger.LogDebug($"* * * Inserting/updating {filteredShifts.Count} shifts in the database.");
            await _shiftsDatabaseContext.SaveChangesAsync();
        }

        _logger.LogDebug($"-- After DB filtering: {filteredShifts.Count} shifts");
        return filteredShifts;
    }

    private async Task ProcessShift(
        ScrapedShift scrapedShift,
        IEnumerable<Shift> existingShifts,
        ICollection<ScrapedShift> filteredShifts
    )
    {
        scrapedShift.TryFillId();

        _logger.LogDebug($"Parsed from url and acquired shift ID: {scrapedShift.Id}");

        if (!scrapedShift.IsValid())
        {
            _logger.LogWarning(
                $"Incoming shift is not valid. Unable to parse Id from: {scrapedShift.DetailUrl} or ShiftDate from {scrapedShift.ShiftDate}"
            );
            return;
        }

        await InsertOrUpdate(scrapedShift, existingShifts, filteredShifts);
    }

    /// <summary>
    /// Inserts shift into database if it doesn't exist or updates it if it does.
    /// Adds to filteredShifts if shift is new or has been updated.
    /// </summary>
    private async Task InsertOrUpdate(
        ScrapedShift scrapedShift,
        IEnumerable<Shift> existingShifts,
        ICollection<ScrapedShift> filteredShifts
    )
    {
        var existingShift = existingShifts.FirstOrDefault(s => s.Id == scrapedShift.Id);
        
        if (existingShift == null)
        {
            _logger.LogDebug($"Has not found existing shift with ID: {scrapedShift.Id}. Inserting...");

            filteredShifts.Add(scrapedShift);

            // Insert new shift into database
            await _shiftsDatabaseContext.Shifts.AddAsync(
                scrapedShift.CreateShift()
            );
        }
        else
        {
            _logger.LogDebug($"Found existing shift with ID {existingShift.Id} in the database.");

            if (existingShift.IsDifferentFrom(scrapedShift))
            {
                _logger.LogDebug($"Existing shift with ID {existingShift.Id} has changed. Updating...");

                filteredShifts.Add(scrapedShift);

                // Update existing shift in the database
                existingShift.UpdateFrom(scrapedShift);
            }
            else
            {
                _logger.LogDebug($"Existing shift with ID {existingShift.Id} has not changed. Skipping.");
            }
        }
    }
}