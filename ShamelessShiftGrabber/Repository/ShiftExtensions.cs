using ShamelessShiftGrabber.Scrape;

namespace ShamelessShiftGrabber.Repository;

internal static class ShiftExtensions
{
    public static bool IsDifferentFrom(this Shift shift, ScrapedShift scrapedShift)
        => shift.Name != scrapedShift.Name ||
           shift.ShiftDate.Date != scrapedShift.ShiftDate ||
           shift.ShiftTime != scrapedShift.ShiftTime;

    public static void UpdateFrom(this Shift shift, ScrapedShift scrapedShift)
    {
        shift.Name = scrapedShift.Name;
        shift.ShiftDate = scrapedShift.ShiftDate;
        shift.ShiftTime = scrapedShift.ShiftTime;
        shift.Modified = DateTime.Now;
    }
}