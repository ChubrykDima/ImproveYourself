using ImproveYourself.Maui.Domain;

namespace ImproveYourself.Maui.Persistence;

public interface ISettingsService
{
    bool ReadOnboardingCompleted();

    void WriteOnboardingCompleted(bool value);

    string ReadCurrentProgramDay();

    void WriteCurrentProgramDay(int value);

    string ReadDisplayName();

    void WriteDisplayName(string value);

    bool ReadNotificationsEnabled();

    void WriteNotificationsEnabled(bool value);

    string ReadBackendBaseUrl();

    void WriteBackendBaseUrl(string value);

    string ReadLanguage();

    void WriteLanguage(string value);

    string ReadBackendClientId();

    SelfAssessmentSnapshot? ReadSelfAssessment(SelfAssessmentKind kind);

    void WriteSelfAssessment(SelfAssessmentSnapshot snapshot);

    void ClearSelfAssessment(SelfAssessmentKind kind);

    string ReadAssessmentDraft(SelfAssessmentKind kind);

    void WriteAssessmentDraft(SelfAssessmentKind kind, string json);

    void ClearAssessmentDraft(SelfAssessmentKind kind);

    string ReadLastSyncedUserId();

    void WriteLastSyncedUserId(string value);

    IReadOnlyList<string> ReadRegressedStepIds();

    void WriteRegressedStepIds(IReadOnlyList<string> stepIds);
}
