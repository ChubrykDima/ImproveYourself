using ImproveYourself.Maui.Domain;

namespace ImproveYourself.Maui.Persistence;

public interface IChallengeRepository
{
    void Initialize();

    void ReloadBundledContent();

    void RelocalizeChallenges();

    void RememberStepTask(string stepId, string? taskKey);

    DailyChallenge? GetChallengeByDate(string date);

    DailyChallenge GetOrCreateChallenge(string date);

    DailyChallenge GetChallengeForProgramDay(int programDay, SelfAssessmentSnapshot? personalizationSnapshot = null);

    DailyChallenge AdvanceChallengeStepStatus(string date, StepType stepType, StepStatus nextStatus);

    IReadOnlyList<string> ListCompletedDates();

    IReadOnlyList<DailyChallenge> ListChallengesBetween(string startDate, string endDate);

    IReadOnlyList<DailyChallenge> ListAllChallenges();

    int GetCompletedChallengesCount();

    void ApplyRemoteProgress(
        IReadOnlyList<RemoteProgramDay> remoteDays,
        bool authoritative,
        IReadOnlySet<string>? locallyRegressedStepIds = null);

    void ResetProgress();
}
