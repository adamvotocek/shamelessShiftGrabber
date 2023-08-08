using ShamelessShiftGrabber.Contracts;

namespace ShamelessShiftGrabber.Repository;

internal static class ApifyShiftExtensions
{
    public static Shift CreateShift(this ApifyShift apifyShift) =>
        new()
        {
            Id = apifyShift.Id,
            Name = apifyShift.Name,
            ShiftDate = apifyShift.ValidDate,
            ShiftTime = apifyShift.ShiftTime,
            Created = DateTime.Now,
            Modified = DateTime.Now
        };
}