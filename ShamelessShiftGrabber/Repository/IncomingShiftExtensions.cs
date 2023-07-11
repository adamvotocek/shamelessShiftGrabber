namespace ShamelessShiftGrabber.Repository;

internal static class IncomingShiftExtensions
{
    public static Shift CreateShift(this IncomingShift incomingShift) =>
        new()
        {
            Id = incomingShift.Id,
            Name = incomingShift.Name,
            ShiftDate = incomingShift.ValidDate,
            ShiftTime = incomingShift.ShiftTime,
            Created = DateTime.Now,
            Modified = DateTime.Now
        };
}