using System.Globalization;

namespace ImproveYourself.Maui.Domain;

internal static class ChallengeTemplateLanguage
{
    private static string _current =
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();

    internal static string Current => _current;

    internal static void SetLanguage(string languageCode)
    {
        _current = languageCode switch
        {
            "ru" => "ru",
            "de" => "de",
            _ => "en",
        };
    }
}
