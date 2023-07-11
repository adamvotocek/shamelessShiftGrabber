using ShamelessShiftGrabber.Contracts;

namespace ShamelessShiftGrabber.Macrodroid;

internal static class IncomingShiftExtensions
{
    public static async Task<bool> Send(
        this IncomingShift incomingShift, 
        string baseUrl,
        HttpClient client,
        ILogger logger)
    {
        var url = $"{baseUrl}?name={incomingShift.Name}" +
                  $"&shiftdate={incomingShift.ShiftDate}" +
                  $"&shifttime={incomingShift.ShiftTime}" +
                  $"&place={incomingShift.Place}" +
                  $"&role={incomingShift.Role}" +
                  $"&occupancy={incomingShift.Occupancy}" +
                  $"&detailurl={incomingShift.DetailUrl}";

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