using ShamelessShiftGrabber.Contracts;

namespace ShamelessShiftGrabber.Repository;

internal static class ShiftExtensions
{
    public static bool IsDifferentFrom(this Shift shift, ApifyShift apifyShift)
        => shift.Name != apifyShift.Name ||
           shift.ShiftDate.Date != apifyShift.ValidDate ||
           shift.ShiftTime != apifyShift.ShiftTime;

    public static void UpdateFrom(this Shift shift, ApifyShift apifyShift)
    {
        shift.Name = apifyShift.Name;
        shift.ShiftDate = apifyShift.ValidDate;
        shift.ShiftTime = apifyShift.ShiftTime;
        shift.Modified = DateTime.Now;
    }
}