using ImproveYourself.Maui.Resources.Strings;

namespace ImproveYourself.Maui.Domain;

public static class ChallengeTextLocalizer
{
    private static readonly string[] KnownDefaultTitles =
    [
        "Your daily challenge",
        "Твой ежедневный вызов",
        "Deine tägliche Herausforderung",
    ];

    public static string GetDisplayTitle(string? title)
    {
        var trimmed = title?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(trimmed) || IsKnownDefaultTitle(trimmed))
        {
            return AppStrings.DailyChallenge_DefaultTitle;
        }

        return trimmed;
    }

    public static bool IsKnownDefaultTitle(string title)
    {
        var trimmed = title.Trim();

        foreach (var known in KnownDefaultTitles)
        {
            if (string.Equals(trimmed, known, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
