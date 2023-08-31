using Microsoft.EntityFrameworkCore;

public class ShiftsDatabaseContext : DbContext
{
    public ShiftsDatabaseContext(DbContextOptions<ShiftsDatabaseContext> options)
        : base(options)
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);

        DbPath = Path.Join(path, "shifts.db");
    }

    public DbSet<Shift> Shifts => Set<Shift>();

    public string DbPath { get; }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
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