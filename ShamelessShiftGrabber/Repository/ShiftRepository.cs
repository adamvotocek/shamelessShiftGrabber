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

    public async Task<ICollection<IncomingShift>> Filter(IncomingShift[] shifts)
    {
        var filteredShifts = new List<IncomingShift>();

        // TODO: Are we getting always incoming shifts in the future?
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
        IncomingShift incomingShift,
        IEnumerable<Shift> existingShifts,
        ICollection<IncomingShift> filteredShifts
    )
    {
        incomingShift.TryFillIdAndValidDate();

        if (!incomingShift.IsValid())
        {
            _logger.LogWarning(
                $"Incoming shift is not valid. Unable to parse Id from: {incomingShift.DetailUrl} or ValidDate from {incomingShift.ShiftDate}"
            );
            return;
        }

        await InsertOrUpdate(incomingShift, existingShifts, filteredShifts);
    }

    /// <summary>
    /// Inserts shift into database if it doesn't exist or updates it if it does.
    /// Adds to filteredShifts if shift is new or has been updated.
    /// </summary>
    private async Task InsertOrUpdate(
        IncomingShift incomingShift,
        IEnumerable<Shift> existingShifts,
        ICollection<IncomingShift> filteredShifts
    )
    {
        var existingShift = existingShifts.FirstOrDefault(s => s.Id == incomingShift.Id);

        if (existingShift == null)
        {
            AddToFilteredShifts(incomingShift, filteredShifts, true);

            // Insert new shift into database
            await _shiftsDatabaseContext.Shifts.AddAsync(
                incomingShift.CreateShift()
            );
        }
        else if (existingShift.IsDifferentFrom(incomingShift))
        {
            // Update existing shift in the database
            AddToFilteredShifts(incomingShift, filteredShifts, false);

            existingShift.UpdateFrom(incomingShift);
        }
    }

    private void AddToFilteredShifts(
        IncomingShift incomingShift, 
        ICollection<IncomingShift> filteredShifts, 
        bool isNewShift
    )
    {
        filteredShifts.Add(incomingShift);

        var msg = isNewShift
            ? "Found new shift"
            : "Found updated shift";

        _logger.LogInformation($"* * * {msg}: {incomingShift.DetailUrl} {incomingShift.Name}");
    }
}