using ImproveYourself.Maui.Domain;

namespace ImproveYourself.Maui.Persistence;

public interface ISettingsService
{
    bool ReadOnboardingCompleted();

    void WriteOnboardingCompleted(bool value);

    string ReadCurrentChallengeDate();

    void WriteCurrentChallengeDate(string value);

    string ReadDisplayName();

    void WriteDisplayName(string value);

    bool ReadNotificationsEnabled();

    void WriteNotificationsEnabled(bool value);

    string ReadBackendBaseUrl();

    void WriteBackendBaseUrl(string value);

    string ReadBackendApiKey();

    void WriteBackendApiKey(string value);

    string ReadLanguage();

    void WriteLanguage(string value);

    string ReadBackendClientId();

    SelfAssessmentSnapshot? ReadSelfAssessment(SelfAssessmentKind kind);

    void WriteSelfAssessment(SelfAssessmentSnapshot snapshot);
}
