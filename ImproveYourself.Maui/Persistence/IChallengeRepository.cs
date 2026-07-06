using ImproveYourself.Maui.Domain;

namespace ImproveYourself.Maui.Persistence;

public interface IChallengeRepository
{
    void Initialize();

    void ReloadBundledContent();

    DailyChallenge? GetChallengeByDate(string date);

    DailyChallenge GetOrCreateChallenge(string date, SelfAssessmentSnapshot? personalizationSnapshot = null);

    void ApplyPersonalization(SelfAssessmentSnapshot snapshot);

    DailyChallenge AdvanceChallengeStepStatus(string date, StepType stepType);

    IReadOnlyList<string> ListCompletedDates();

    IReadOnlyList<DailyChallenge> ListChallengesBetween(string startDate, string endDate);

    int GetCompletedChallengesCount();
}
