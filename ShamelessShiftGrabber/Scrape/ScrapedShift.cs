using System.Globalization;

namespace ShamelessShiftGrabber.Scrape;

public class ScrapedShift
{
    public string Name { get; set; }

    public string ShiftTime { get; set; }

    public string Place { get; set; }

    public string Role { get; set; }

    public string Occupancy { get; set; }

    public string DetailUrl { get; set; }

    public int Id { get; private set; }

    public DateTime ShiftDate { get; private set; }

    /// <summary>
    /// Sets Id from scraped shift.
    /// (Retrieves Id from input DetailUrl (e.g. /react/position/1234) and parses input string ShiftDate (e.g. 13. 5. 2023) into DateTime.)
    /// </summary>
    public void TryFillId()
    {
        var parsedShiftId = DetailUrl.Split('/').Last();
        Id = int.TryParse(parsedShiftId, out var shiftId) ? shiftId : default;
    }
    
    public void TryFillShiftDate(string inputDate)
    {
        ShiftDate = DateTime.TryParse(inputDate, CultureInfo.GetCultureInfo("cs-CZ"), out var parsedShiftDate)
            ? parsedShiftDate
            : default;
    }

    public bool IsValid() => Id != default && ShiftDate != default;
}