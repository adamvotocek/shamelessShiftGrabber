using ShamelessShiftGrabber.Contracts;

namespace ShamelessShiftGrabber.Macrodroid;

internal static class ScrapedShiftExtensions
{
    public static async Task<bool> Send(
        this ScrapedShift shift,
        string baseUrl,
        HttpClient client,
        ILogger logger)
    {
        var url = $"{baseUrl}?name={shift.Name}" +
                  $"&shiftdate={shift.ShiftDate}" +
                  $"&shifttime={shift.ShiftTime}" +
                  $"&place={shift.Place}" +
                  $"&role={shift.Role}" +
                  $"&occupancy={shift.Occupancy}" +
                  $"&detailurl={shift.DetailUrl}";

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