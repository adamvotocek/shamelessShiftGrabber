namespace ShamelessShiftGrabber.Scrape;

public class ScrapingConfiguration
{
    public ScrapingConfiguration(IConfiguration configuration)
    {
        configuration.Bind("Shameless", this);
    }

    public string UserName { get; set; }
    public string Password { get; set; }
}