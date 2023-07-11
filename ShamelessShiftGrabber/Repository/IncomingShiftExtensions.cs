using System.Globalization;

namespace ShamelessShiftGrabber.Repository;

internal static class IncomingShiftExtensions
{
    /// <summary>
    /// Gets shift id from incoming shift detailUrl (e.g. /react/position/1234)
    /// </summary>
    public static int GetShiftId(this IncomingShift incomingShift)
    {
        var parsedShiftId = incomingShift.DetailUrl.Split('/').Last();

        return int.TryParse(parsedShiftId, out var shiftId) ? shiftId : default;
    }

    /// <summary>
    /// Parses incoming shift string date (e.g. 13. 5. 2023) and returns appropriate DateTime variable.
    /// </summary>
    public static DateTime GetShiftDate(this IncomingShift incomingShift) =>
        DateTime.TryParse(incomingShift.ShiftDate, CultureInfo.GetCultureInfo("cs-CZ"), out var parsedShiftDate)
            ? parsedShiftDate
            : default;
}