using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace ShamelessShiftGrabber;

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
        var shiftId = GetShiftId(incomingShift);
        if (shiftId == default)
        {
            _logger.LogWarning($"Failed to parse shift ID from incoming shift DetailUrl: {incomingShift.DetailUrl}");
            return;
        }

        var shiftDate = GetShiftDate(incomingShift);
        if (shiftDate == default)
        {
            _logger.LogWarning($"Failed to parse shift date from incoming shift date: {incomingShift.ShiftDate}");
            return;
        }

        await InsertOrUpdate(incomingShift, existingShifts, filteredShifts, shiftId, shiftDate);
    }

    /// <summary>
    /// Inserts shift into database if it doesn't exist or updates it if it does.
    /// Adds to filteredShifts if shift is new or has been updated.
    /// </summary>
    private async Task InsertOrUpdate(
        IncomingShift incomingShift,
        IEnumerable<Shift> existingShifts,
        ICollection<IncomingShift> filteredShifts,
        int shiftId,
        DateTime shiftDate
    )
    {
        var foundShift = existingShifts.FirstOrDefault(s => s.Id == shiftId);

        if (foundShift == null)
        {
            filteredShifts.Add(incomingShift);
            _logger.LogInformation($"* * * Found new shift: {incomingShift.DetailUrl} {incomingShift.Name}");

            // Insert shift into database
            await _shiftsDatabaseContext.Shifts.AddAsync(
                new Shift
                {
                    Id = shiftId,
                    Name = incomingShift.Name,
                    ShiftDate = shiftDate,
                    ShiftTime = incomingShift.ShiftTime,
                    Created = DateTime.Now,
                    Modified = DateTime.Now
                }
            );
        }
        else if (foundShift.Name != incomingShift.Name ||
                 foundShift.ShiftDate.Date != shiftDate.Date ||
                 foundShift.ShiftTime != incomingShift.ShiftTime)
        {
            // Update shift in the database
            filteredShifts.Add(incomingShift);
            _logger.LogInformation($"* * * Found updated shift: {incomingShift.DetailUrl} {incomingShift.Name}");

            foundShift.Name = incomingShift.Name;
            foundShift.ShiftDate = shiftDate;
            foundShift.ShiftTime = incomingShift.ShiftTime;
            foundShift.Modified = DateTime.Now;
        }
    }

    /// <summary>
    /// Gets shift id from incoming shift detailUrl (e.g. /react/position/1234)
    /// </summary>
    private static int GetShiftId(IncomingShift incomingShift)
    {
        var parsedShiftId = incomingShift.DetailUrl.Split('/').Last();

        return int.TryParse(parsedShiftId, out var shiftId) ? shiftId : default;
    }

    /// <summary>
    /// Parses incoming shift string date (e.g. 13. 5. 2023) and returns appropriate DateTime variable.
    /// </summary>
    /// <param name="incomingShift"></param>
    /// <returns></returns>
    private static DateTime GetShiftDate(IncomingShift incomingShift) =>
        DateTime.TryParse(incomingShift.ShiftDate, CultureInfo.GetCultureInfo("cs-CZ"), out var parsedShiftDate)
            ? parsedShiftDate
            : default;
}