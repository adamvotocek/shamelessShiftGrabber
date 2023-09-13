using System.Globalization;

namespace ShamelessShiftGrabber.GoogleSheets;

public static class SheetItemMapper
{
    public static List<SheetConditionItem> ToSheetConditionItems(IList<IList<object>> values)
    {
        var items = new List<SheetConditionItem>();

        foreach (var value in values.Skip(1))
        {
            var sheetItem = new SheetConditionItem
            {
                Condition = value[0].ToString(),
            };

            items.Add(sheetItem);
        }

        return items;
    }

    public static List<SheetAvailableDateItem> ToSheetAvailableDateItems(IList<IList<object>> values)
    {
        var items = new List<SheetAvailableDateItem>();

        foreach (var value in values.Skip(1))
        {
            var sheetItem = new SheetAvailableDateItem();
            
            if (DateTime.TryParseExact(value[0].ToString(), "d.M.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            {
                sheetItem.AvailableDate = parsedDate;
            }

            items.Add(sheetItem);
        }

        return items;
    }
}