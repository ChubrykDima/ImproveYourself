using Microsoft.Maui.Storage;
using System.Text.Json;
using System.Text.Json.Serialization;
using ImproveYourself.Maui.Domain;

namespace ImproveYourself.Maui.Persistence;

public sealed class PreferencesSettingsService : ISettingsService
{
    private const string OnboardingCompletedKey = "onboardingCompleted";
    private const string CurrentChallengeDateKey = "currentChallengeDate";
    private const string DisplayNameKey = "displayName";
    private const string NotificationsEnabledKey = "notificationsEnabled";
    private const string BackendBaseUrlKey = "backend.baseUrl";
    private const string BackendApiKeyKey = "backend.apiKey";
    private const string LanguageKey = "language";
    private const string StartSelfAssessmentKey = "selfAssessment.start";
    private const string FinalSelfAssessmentKey = "selfAssessment.final";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public bool ReadOnboardingCompleted() =>
        Preferences.Default.Get(OnboardingCompletedKey, false);

    public void WriteOnboardingCompleted(bool value) =>
        Preferences.Default.Set(OnboardingCompletedKey, value);

    public string ReadCurrentChallengeDate() =>
        Preferences.Default.Get(CurrentChallengeDateKey, string.Empty);

    public void WriteCurrentChallengeDate(string value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        Preferences.Default.Set(CurrentChallengeDateKey, normalized);
    }

    public string ReadDisplayName() =>
        Preferences.Default.Get(DisplayNameKey, string.Empty);

    public void WriteDisplayName(string value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        Preferences.Default.Set(DisplayNameKey, normalized);
    }

    public bool ReadNotificationsEnabled() =>
        Preferences.Default.Get(NotificationsEnabledKey, false);

    public void WriteNotificationsEnabled(bool value) =>
        Preferences.Default.Set(NotificationsEnabledKey, value);

    public string ReadBackendBaseUrl() =>
        Preferences.Default.Get(BackendBaseUrlKey, string.Empty);

    public void WriteBackendBaseUrl(string value)
    {
        var normalized = string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().TrimEnd('/');

        Preferences.Default.Set(BackendBaseUrlKey, normalized);
    }

    public string ReadBackendApiKey() =>
        Preferences.Default.Get(BackendApiKeyKey, string.Empty);

    public void WriteBackendApiKey(string value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        Preferences.Default.Set(BackendApiKeyKey, normalized);
    }

    public string ReadLanguage() =>
        Preferences.Default.Get(LanguageKey, string.Empty);

    public void WriteLanguage(string value) =>
        Preferences.Default.Set(LanguageKey, value?.Trim() ?? string.Empty);

    public SelfAssessmentSnapshot? ReadSelfAssessment(SelfAssessmentKind kind)
    {
        var json = Preferences.Default.Get(GetSelfAssessmentKey(kind), string.Empty);

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<SelfAssessmentSnapshot>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public void WriteSelfAssessment(SelfAssessmentSnapshot snapshot)
    {
        var json = JsonSerializer.Serialize(snapshot, JsonOptions);
        Preferences.Default.Set(GetSelfAssessmentKey(snapshot.Kind), json);
    }

    private static string GetSelfAssessmentKey(SelfAssessmentKind kind) => kind switch
    {
        SelfAssessmentKind.Start => StartSelfAssessmentKey,
        SelfAssessmentKind.Final => FinalSelfAssessmentKey,
        _ => StartSelfAssessmentKey,
    };
}
