using ShamelessShiftGrabber.Contracts;

namespace ShamelessShiftGrabber.Repository;

internal static class ShiftExtensions
{
    public static bool IsDifferentFrom(this Shift shift, IncomingShift incomingShift)
        => shift.Name != incomingShift.Name ||
           shift.ShiftDate.Date != incomingShift.ValidDate ||
           shift.ShiftTime != incomingShift.ShiftTime;

    public static void UpdateFrom(this Shift shift, IncomingShift incomingShift)
    {
        shift.Name = incomingShift.Name;
        shift.ShiftDate = incomingShift.ValidDate;
        shift.ShiftTime = incomingShift.ShiftTime;
        shift.Modified = DateTime.Now;
    }
}