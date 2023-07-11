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

    public async Task<IResult> Send(ICollection<IncomingShift> shifts)
    {
        if (shifts.Count == 0)
        {
            const string noShiftsMessage = "* * * No shifts to send to Macrodroid";
            _logger.LogInformation(noShiftsMessage);

            return Results.Ok(noShiftsMessage);
        }

        var successSendCount = 0;

        foreach (var shift in shifts)
        {
            var isShiftSentOk = await shift.Send(_baseUrl, _client, _logger);
            if (isShiftSentOk)
            {
                successSendCount++;
            }
        }

        var successMessage = $"* * * Successfully sent {successSendCount}/{shifts.Count} shifts to Macrodroid";
        _logger.LogInformation(successMessage);

        if (successSendCount > 0)
        {
            return Results.Ok(successMessage);
        }

        return Results.BadRequest("Failed to send all shifts to Macrodroid");
    }
}