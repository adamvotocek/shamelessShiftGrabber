using System.Globalization;

namespace ShamelessShiftGrabber;

internal class IncomingShift
{
    public string Name { get; set; }
    public string ShiftDate { get; set; }
    public string ShiftTime { get; set; }
    public string Place { get; set; }
    public string Role { get; set; }
    public string Occupancy { get; set; }
    public string DetailUrl { get; set; }

    public int Id { get; private set; }
    public DateTime ValidDate { get; private set; }

    /// <summary>
    /// Gets Id and ValidDate from incoming shift.
    /// (Retrieves Id from input DetailUrl (e.g. /react/position/1234) and parses input string ShiftDate (e.g. 13. 5. 2023) into DateTime.)
    /// </summary>
    public void TryFillIdAndValidDate()
    {
        var parsedShiftId = DetailUrl.Split('/').Last();
        Id = int.TryParse(parsedShiftId, out var shiftId) ? shiftId : default;

        ValidDate = DateTime.TryParse(ShiftDate, CultureInfo.GetCultureInfo("cs-CZ"), out var parsedShiftDate)
            ? parsedShiftDate
            : default;
    }

    public bool IsValid() => Id != default && ValidDate != default;
}