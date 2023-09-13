namespace ShamelessShiftGrabber.GoogleSheets;

public static class SheetAvailableDateItemExtensions
{
    public static List<DateTime> GetAvailableDates(this List<SheetAvailableDateItem> sheetAvailableDate)
    {
        var availableDates = sheetAvailableDate
            .Where(x => x.AvailableDate.HasValue)
            .Select(x => x.AvailableDate.Value)
            .ToList();
        return availableDates;
    }
}