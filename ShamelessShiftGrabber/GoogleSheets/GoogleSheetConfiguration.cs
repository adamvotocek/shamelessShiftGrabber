namespace ShamelessShiftGrabber.GoogleSheets;

public class GoogleSheetConfiguration
{
    public GoogleSheetConfiguration(IConfiguration configuration)
    {
        configuration.Bind("GoogleSheet", this);
    }

    public string SpreadSheetId { get; set; }
    public string SheetName { get; set; }
}