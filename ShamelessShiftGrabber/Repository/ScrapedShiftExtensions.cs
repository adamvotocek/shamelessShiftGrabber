using ShamelessShiftGrabber.Scrape;

namespace ShamelessShiftGrabber.Repository;

internal static class ScrapedShiftExtensions
{
    public static Shift CreateShift(this ScrapedShift shift) =>
        new()
        {
            Id = shift.Id,
            Name = shift.Name,
            ShiftDate = shift.ShiftDate,
            ShiftTime = shift.ShiftTime,
            Created = DateTime.Now,
            Modified = DateTime.Now
        };
}