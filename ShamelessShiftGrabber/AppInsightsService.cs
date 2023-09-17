using Microsoft.ApplicationInsights;

namespace ShamelessShiftGrabber;

public class AppInsightsService
{
    private readonly TelemetryClient _telemetryClient;

    public AppInsightsService(TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
    }

    public void TrackException(Exception exception)
    {
        _telemetryClient.TrackException(exception);
    }

    public void TrackError(string error)
    {
        _telemetryClient.TrackException(
            new ApplicationException(error)
        );
    }
}