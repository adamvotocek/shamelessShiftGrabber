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

    public async Task<ICollection<ApifyShift>> Filter(List<ApifyShift> shifts)
    {
        var filteredShifts = new List<ApifyShift>();

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
        ApifyShift apifyShift,
        IEnumerable<Shift> existingShifts,
        ICollection<ApifyShift> filteredShifts
    )
    {
        apifyShift.TryFillIdAndValidDate();

        if (!apifyShift.IsValid())
        {
            _logger.LogWarning(
                $"Incoming shift is not valid. Unable to parse Id from: {apifyShift.DetailUrl} or ValidDate from {apifyShift.ShiftDate}"
            );
            return;
        }

        await InsertOrUpdate(apifyShift, existingShifts, filteredShifts);
    }

    /// <summary>
    /// Inserts shift into database if it doesn't exist or updates it if it does.
    /// Adds to filteredShifts if shift is new or has been updated.
    /// </summary>
    private async Task InsertOrUpdate(
        ApifyShift apifyShift,
        IEnumerable<Shift> existingShifts,
        ICollection<ApifyShift> filteredShifts
    )
    {
        var existingShift = existingShifts.FirstOrDefault(s => s.Id == apifyShift.Id);

        if (existingShift == null)
        {
            AddToFilteredShifts(apifyShift, filteredShifts, true);

            // Insert new shift into database
            await _shiftsDatabaseContext.Shifts.AddAsync(
                apifyShift.CreateShift()
            );
        }
        else if (existingShift.IsDifferentFrom(apifyShift))
        {
            // Update existing shift in the database
            AddToFilteredShifts(apifyShift, filteredShifts, false);

            existingShift.UpdateFrom(apifyShift);
        }
    }

    private void AddToFilteredShifts(
        ApifyShift apifyShift, 
        ICollection<ApifyShift> filteredShifts, 
        bool isNewShift
    )
    {
        filteredShifts.Add(apifyShift);

        var msg = isNewShift
            ? "Found new shift"
            : "Found updated shift";

        _logger.LogInformation($"* * * {msg}: {apifyShift.DetailUrl} {apifyShift.Name}");
    }
}