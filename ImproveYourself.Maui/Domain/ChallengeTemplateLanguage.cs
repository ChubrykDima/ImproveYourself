using System.Globalization;

namespace ImproveYourself.Maui.Domain;

internal static class ChallengeTemplateLanguage
{
    internal static string Current =>
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
}
