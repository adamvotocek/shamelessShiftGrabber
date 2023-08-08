using ShamelessShiftGrabber.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ShamelessShiftGrabber.Apify;

internal class Apify
{
    private readonly ILogger<Apify> _logger;
    private readonly string _token;
    private readonly HttpClient _client;

    public Apify(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<Apify> logger
    )
    {
        _logger = logger;
        _token = configuration.GetValue<string>("ApifyToken");
        _client = httpClientFactory.CreateClient("apify");
    }

    public async Task<List<ApifyShift>> GetScrapedShifts(string runId)
    {
        var actorRunId = await GetActorRunId(runId);
        if (string.IsNullOrWhiteSpace(actorRunId))
        {
            _logger.LogError("Failed to get apify actor run id");

            return new List<ApifyShift>();
        }

        var shifts = await GitShifts(actorRunId);

        return shifts.ToList();
    }

    private async Task<string> GetActorRunId(string runId)
    {
        var url = $"actor-runs/{runId}/dataset?token={_token}";

        var response = await _client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var parsedObject = JObject.Parse(jsonResponse);
            var id = parsedObject["data"]?["id"]?.ToString();

            _logger.LogInformation($"Found apify actor run id: {id}");

            return id;

        }

        var error =
            $"Failed to get apify actor run. Returned status code: {response.StatusCode}, reason: {response.ReasonPhrase}, message: {response.RequestMessage}";

        _logger.LogError(error);

        return null;
    }

    private async Task<IEnumerable<ApifyShift>> GitShifts(string datasetId)
    {
        var url = $"datasets/{datasetId}/items?clean=true&format=jsonl";

        var response = await _client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var responseBody = JsonConvert.DeserializeObject<ApifyShiftResponse>(jsonResponse);

            if (responseBody is { Shifts: { } })
            {
                _logger.LogInformation($"Found {responseBody.Shifts.Count} shifts in apify dataset {datasetId}");

                return responseBody.Shifts.ToList();
            }

            _logger.LogInformation($"No shifts found in apify dataset {datasetId}");

            return Enumerable.Empty<ApifyShift>();
        }

        var error =
            $"Failed to get apify dataset. Returned status code: {response.StatusCode}, reason: {response.ReasonPhrase}, message: {response.RequestMessage}";

        _logger.LogError(error);

        return Enumerable.Empty<ApifyShift>();
    }
}