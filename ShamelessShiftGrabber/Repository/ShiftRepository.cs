using Microsoft.EntityFrameworkCore;
using ShamelessShiftGrabber.Contracts;

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

    public async Task<ICollection<ScrapedShift>> Filter(List<ScrapedShift> shifts)
    {
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
            _logger.LogInformation($"* * * Inserting/updating {filteredShifts.Count} shifts in the database.");
            await _shiftsDatabaseContext.SaveChangesAsync();
        }

        return filteredShifts;
    }

    private async Task ProcessShift(
        ScrapedShift scrapedShift,
        IEnumerable<Shift> existingShifts,
        ICollection<ScrapedShift> filteredShifts
    )
    {
        scrapedShift.TryFillIdAndValidDate();

        if (!scrapedShift.IsValid())
        {
            _logger.LogWarning(
                $"Incoming shift is not valid. Unable to parse Id from: {scrapedShift.DetailUrl} or ValidDate from {scrapedShift.ShiftDate}"
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
            AddToFilteredShifts(scrapedShift, filteredShifts, true);

            // Insert new shift into database
            await _shiftsDatabaseContext.Shifts.AddAsync(
                scrapedShift.CreateShift()
            );
        }
        else if (existingShift.IsDifferentFrom(scrapedShift))
        {
            // Update existing shift in the database
            AddToFilteredShifts(scrapedShift, filteredShifts, false);

            existingShift.UpdateFrom(scrapedShift);
        }
    }

    private void AddToFilteredShifts(
        ScrapedShift scrapedShift,
        ICollection<ScrapedShift> filteredShifts,
        bool isNewShift
    )
    {
        filteredShifts.Add(scrapedShift);

        var msg = isNewShift
            ? "Found new shift"
            : "Found updated shift";

        _logger.LogInformation($"* * * {msg}: {scrapedShift.DetailUrl} {scrapedShift.Name}");
    }
}