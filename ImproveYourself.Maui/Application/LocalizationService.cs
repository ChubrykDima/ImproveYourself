using System.Globalization;
using System.Text.Json;
using ImproveYourself.Maui.Persistence;
using ImproveYourself.Maui.Resources.Strings;

namespace ImproveYourself.Maui.Application;

public sealed class LocalizationService : ILocalizationService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private static readonly IReadOnlyList<string> SupportedLanguages = ["en", "ru", "de"];
    private static readonly string FallbackLanguage = "en";

    private readonly ISettingsService _settingsService;

    public string CurrentLanguage { get; private set; } = FallbackLanguage;

    public LocalizationService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public void Initialize()
    {
        var saved = _settingsService.ReadLanguage();
        var language = ResolveLanguage(saved);
        ApplyLanguage(language);
    }

    public void SetLanguage(string languageCode)
    {
        var language = ResolveLanguage(languageCode);
        _settingsService.WriteLanguage(language);
        ApplyLanguage(language);
    }

    private void ApplyLanguage(string language)
    {
        CurrentLanguage = language;

        var strings = LoadStrings(language);
        AppStrings.Load(strings);

        var culture = GetCultureForLanguage(language);
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
    }

    private static CultureInfo GetCultureForLanguage(string language) =>
        language switch
        {
            "ru" => CultureInfo.GetCultureInfo("ru-RU"),
            "de" => CultureInfo.GetCultureInfo("de-DE"),
            _ => CultureInfo.GetCultureInfo("en-US"),
        };

    private static IReadOnlyDictionary<string, string> LoadStrings(string language)
    {
        try
        {
            using var stream = FileSystem.OpenAppPackageFileAsync($"strings/{language}.json").GetAwaiter().GetResult();
            var result = JsonSerializer.Deserialize<Dictionary<string, string>>(stream, JsonOptions);

            return result ?? new Dictionary<string, string>(StringComparer.Ordinal);
        }
        catch
        {
            if (language != FallbackLanguage)
            {
                return LoadStrings(FallbackLanguage);
            }

            return new Dictionary<string, string>(StringComparer.Ordinal);
        }
    }

    private static string ResolveLanguage(string? requested)
    {
        if (!string.IsNullOrWhiteSpace(requested) && SupportedLanguages.Contains(requested))
        {
            return requested;
        }

        var systemLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();

        return SupportedLanguages.Contains(systemLanguage) ? systemLanguage : FallbackLanguage;
    }
}
