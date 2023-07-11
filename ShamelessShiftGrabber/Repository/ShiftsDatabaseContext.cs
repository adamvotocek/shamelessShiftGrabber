using Microsoft.EntityFrameworkCore;

public class ShiftsDatabaseContext : DbContext
{
    public ShiftsDatabaseContext(DbContextOptions<ShiftsDatabaseContext> options)
        : base(options) { }

    public DbSet<Shift> Shifts => Set<Shift>();
}

public class Shift
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime ShiftDate { get; set; }
    public string ShiftTime { get; set; }
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }
}