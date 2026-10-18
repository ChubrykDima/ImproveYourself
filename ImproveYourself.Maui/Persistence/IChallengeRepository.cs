using ImproveYourself.Maui.Domain;

namespace ImproveYourself.Maui.Persistence;

public interface IChallengeRepository
{
    void Initialize();

    void ReloadBundledContent();

    void RelocalizeChallenges();

    DailyChallenge? GetChallengeByDate(string date);

    DailyChallenge GetOrCreateChallenge(string date);

    DailyChallenge GetChallengeForProgramDay(int programDay, SelfAssessmentSnapshot? personalizationSnapshot = null);

    DailyChallenge AdvanceChallengeStepStatus(string date, StepType stepType);

    IReadOnlyList<string> ListCompletedDates();

    IReadOnlyList<DailyChallenge> ListChallengesBetween(string startDate, string endDate);

    IReadOnlyList<DailyChallenge> ListAllChallenges();

    int GetCompletedChallengesCount();
}
