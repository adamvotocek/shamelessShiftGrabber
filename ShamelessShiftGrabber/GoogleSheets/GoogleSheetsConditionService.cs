using Google.Apis.Sheets.v4;

namespace ShamelessShiftGrabber.GoogleSheets;

public class GoogleSheetsConditionService
{
    private readonly GoogleSheetConfiguration _googleSheetConfiguration;
    private readonly SpreadsheetsResource.ValuesResource _googleSheetValues;
    private readonly ILogger<GoogleSheetsConditionService> _logger;

    public GoogleSheetsConditionService(
        GoogleSheetConfiguration googleSheetConfiguration,  
        GoogleSheets googleSheets, ILogger<GoogleSheetsConditionService> logger)
    {
        _googleSheetConfiguration = googleSheetConfiguration;
        _logger = logger;
        _googleSheetValues = googleSheets.Service.Spreadsheets.Values;
    }

    public SheetInformation GetSheetInformation()
    {
        var values = GetSheetValues("B:B");
        var sheetConditionItems = SheetItemMapper.ToSheetConditionItems(values);

        if (sheetConditionItems.Count != 2)
        {
            _logger.LogError("Failed to get Google sheets css conditions. There must be 2 css conditions in the sheet");
            return null;
        }

        values = GetSheetValues("A:A");
        var sheetAvailableDateItems = SheetItemMapper.ToSheetAvailableDateItems(values);

        var sheetInformation = new SheetInformation
        {
            Condition = sheetConditionItems[0].Condition,
            ConditionIds = sheetConditionItems[1].Condition,
            AvailableDates = sheetAvailableDateItems.GetAvailableDates()
        };

        _logger.LogDebug($"Sheet available dates count: {sheetInformation.AvailableDates.Count}");
        _logger.LogDebug($"Css condition: {sheetInformation.Condition}");
        _logger.LogDebug($"Css conditionIds: {sheetInformation.ConditionIds}");

        return sheetInformation;
    }

    private IList<IList<object>> GetSheetValues(string rangePart)
    {
        var range = $"{_googleSheetConfiguration.SheetName}!{rangePart}";
        var request = _googleSheetValues.Get(_googleSheetConfiguration.SpreadSheetId, range);

        var response = request.Execute();
        var values = response.Values;

        return values;
    }
}
