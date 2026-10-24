using Microsoft.Maui.Storage;

namespace ImproveYourself.Maui.Persistence;

public sealed class InstallationSessionGuard(IAuthTokenStore tokenStore)
{
    private const string InstallationMarkerKey = "installation.sessionGuard.v1";

    private static readonly string[] LegacyInstallationKeys =
    [
        "onboardingCompleted",
        "currentProgramDay",
        "currentChallengeDate",
        "displayName",
        "notificationsEnabled",
        "backend.baseUrl",
        "language",
        "backend.clientId",
        "selfAssessment.start",
        "selfAssessment.final",
    ];

    public async Task PrepareAsync()
    {
        if (Preferences.Default.Get(InstallationMarkerKey, false))
        {
            return;
        }

        var isExistingInstallation = LegacyInstallationKeys.Any(key => Preferences.Default.ContainsKey(key));

        if (!isExistingInstallation)
        {
            // iOS Keychain entries can survive app deletion while Preferences and app data do not.
            // Do not silently restore that previous installation's account into a fresh install.
            await tokenStore.ClearAsync();
        }

        Preferences.Default.Set(InstallationMarkerKey, true);
    }
}
