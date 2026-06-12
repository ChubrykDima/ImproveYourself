using Microsoft.Maui.Storage;

namespace ImproveYourself.Maui.Persistence;

public sealed class PreferencesSettingsService : ISettingsService
{
    private const string OnboardingCompletedKey = "onboardingCompleted";
    private const string CurrentChallengeDateKey = "currentChallengeDate";
    private const string DisplayNameKey = "displayName";
    private const string NotificationsEnabledKey = "notificationsEnabled";

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
        Preferences.Default.Get(DisplayNameKey, "Друг");

    public void WriteDisplayName(string value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? "Друг" : value.Trim();
        Preferences.Default.Set(DisplayNameKey, normalized);
    }

    public bool ReadNotificationsEnabled() =>
        Preferences.Default.Get(NotificationsEnabledKey, false);

    public void WriteNotificationsEnabled(bool value) =>
        Preferences.Default.Set(NotificationsEnabledKey, value);
}
