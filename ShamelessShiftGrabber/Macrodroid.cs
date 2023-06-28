using Microsoft.AspNetCore.Http.HttpResults;

namespace ShamelessShiftGrabber;

internal class Macrodroid
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<Macrodroid> _logger;

    public Macrodroid(
        IHttpClientFactory httpClientFactory, 
        IConfiguration configuration,
        ILogger<Macrodroid> logger
    )
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<IResult> Send(Shift[] shifts)
    {
        var macrodroidDeviceId = _configuration.GetValue<string>("MacrodroidDeviceId");
        var macrodroidEndpoint = _configuration.GetValue<string>("MacrodroidEndpoint");

        var url = $"{macrodroidDeviceId}/{macrodroidEndpoint}";
        var client = _httpClientFactory.CreateClient("macrodroid");

        foreach (var shift in shifts)
        {
            //await SendSingle(shift);
        }
        
        var response = await client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Successfully sent shifts to macrodroid");
            return Results.Ok();
        }

        var error =
            $"Failed to send shifts. Macrodroid returned status code: {response.StatusCode}, reason: {response.ReasonPhrase}, message: {response.RequestMessage}";

        _logger.LogError(error);

        return Results.BadRequest(error);
    }
}