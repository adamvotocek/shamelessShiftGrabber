using ShamelessShiftGrabber.Contracts;

namespace ShamelessShiftGrabber.Macrodroid;

internal static class ApifyShiftExtensions
{
    public static async Task<bool> Send(
        this ApifyShift apifyShift, 
        string baseUrl,
        HttpClient client,
        ILogger logger)
    {
        var url = $"{baseUrl}?name={apifyShift.Name}" +
                  $"&shiftdate={apifyShift.ShiftDate}" +
                  $"&shifttime={apifyShift.ShiftTime}" +
                  $"&place={apifyShift.Place}" +
                  $"&role={apifyShift.Role}" +
                  $"&occupancy={apifyShift.Occupancy}" +
                  $"&detailurl={apifyShift.DetailUrl}";

        var response = await client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            return true;
        }

        var error =
            $"Failed to send shifts. Macrodroid returned status code: {response.StatusCode}, reason: {response.ReasonPhrase}, message: {response.RequestMessage}";

        logger.LogError(error);

        return false;
    }
}