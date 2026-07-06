using System.Globalization;

namespace ImproveYourself.Maui.Domain;

public static class DateHelpers
{
    public const int TargetMonthlyDays = 30;

    public static string ToIsoDate(DateTime date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    public static string AddDays(string isoDate, int days) =>
        ParseIsoDate(isoDate).AddDays(days).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    public static string ToDisplayDate(string isoDate) =>
        ParseIsoDate(isoDate).ToString("d MMMM", CultureInfo.CurrentCulture);

    public static DateOnly ParseIsoDate(string isoDate) =>
        DateOnly.ParseExact(isoDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

    public static bool TryParseIsoDate(string isoDate, out DateOnly date) =>
        DateOnly.TryParseExact(
            isoDate,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out date);

    public static string MonthKeyFromDate(DateTime date) => date.ToString("yyyy-MM", CultureInfo.InvariantCulture);

    public static string MonthKeyFromIsoDate(string isoDate) =>
        isoDate.Length >= 7 ? isoDate[..7] : string.Empty;
}
