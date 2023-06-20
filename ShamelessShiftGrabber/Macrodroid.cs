namespace ShamelessShiftGrabber;

internal class Macrodroid
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public Macrodroid(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task Send(Shift[] shifts)
    {
        var macrodroidDeviceId = _configuration.GetValue<string>("MacrodroidDeviceId");

        var url = $"{macrodroidDeviceId}/gug";
        var client = _httpClientFactory.CreateClient("macrodroid");

        foreach (var shift in shifts)
        {
            //await SendSingle(shift);
        }
        
        var response = await client.GetAsync(url);

        Console.WriteLine(response.StatusCode);
    }
}