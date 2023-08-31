using ShamelessShiftGrabber.Contracts;

namespace ShamelessShiftGrabber.Macrodroid;

internal class Macrodroid
{
    private readonly ILogger<Macrodroid> _logger;
    private readonly string _baseUrl;
    private readonly HttpClient _client;

    public Macrodroid(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<Macrodroid> logger
    )
    {
        _logger = logger;

        var macrodroidDeviceId = configuration.GetValue<string>("MacrodroidDeviceId");
        var macrodroidEndpoint = configuration.GetValue<string>("MacrodroidEndpoint");

        _baseUrl = $"{macrodroidDeviceId}/{macrodroidEndpoint}";
        _client = httpClientFactory.CreateClient("macrodroid");
    }

    public async Task<bool> Send(ICollection<ScrapedShift> shifts)
    {
        var successSendCount = 0;

        foreach (var shift in shifts)
        {
            var isShiftSentOk = await shift.Send(_baseUrl, _client, _logger);
            if (isShiftSentOk)
            {
                successSendCount++;
            }
        }

        _logger.LogInformation($"* * * Successfully sent {successSendCount}/{shifts.Count} shifts to Macrodroid");

        return successSendCount > 0;
    }
}