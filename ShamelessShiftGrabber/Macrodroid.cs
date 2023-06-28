using Microsoft.AspNetCore.Http.HttpResults;

namespace ShamelessShiftGrabber;

internal class Macrodroid
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<Macrodroid> _logger;

    private readonly string _baseUrl;
    private readonly HttpClient _client;

    public Macrodroid(
        IHttpClientFactory httpClientFactory, 
        IConfiguration configuration,
        ILogger<Macrodroid> logger
    )
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
        
        var macrodroidDeviceId = _configuration.GetValue<string>("MacrodroidDeviceId");
        var macrodroidEndpoint = _configuration.GetValue<string>("MacrodroidEndpoint");

        _baseUrl = $"{macrodroidDeviceId}/{macrodroidEndpoint}";
        _client = _httpClientFactory.CreateClient("macrodroid");
    }

    public async Task<IResult> Send(Shift[] shifts)
    {
        var successSendCount = 0;

        foreach (var shift in shifts)
        {
            var sendResult = await SendSingle(shift);

            if (sendResult)
            {
                successSendCount++;
            }
        }
        
        var successMessage = $"Successfully sent {successSendCount}/{shifts.Length} shifts to Macrodroid";

        _logger.LogInformation(successMessage);

        if (successSendCount > 0)
        {
            return Results.Ok(successMessage);
        }

        return Results.BadRequest("Failed to send all shifts to Macrodroid");
    }

    private async Task<bool> SendSingle(Shift shift)
    {
        var url = $"{_baseUrl}?name={shift.Name}&shiftdate={shift.ShiftDate}&shifttime={shift.ShiftTime}&place={shift.Place}&role={shift.Role}&occupancy={shift.Occupancy}";

        var response = await _client.GetAsync(url);
        
        if (response.IsSuccessStatusCode)
        {
            return true;
        }

        var error =
            $"Failed to send shifts. Macrodroid returned status code: {response.StatusCode}, reason: {response.ReasonPhrase}, message: {response.RequestMessage}";

        _logger.LogError(error);

        return false;
    }
}