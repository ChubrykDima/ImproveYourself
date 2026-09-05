using ImproveYourself.Maui.Domain;

namespace ImproveYourself.Maui.Persistence;

public interface IChallengeRepository
{
    void Initialize();

    void ReloadBundledContent();

    void RelocalizeChallenges(SelfAssessmentSnapshot? personalizationSnapshot);

    DailyChallenge? GetChallengeByDate(string date);

    DailyChallenge GetOrCreateChallenge(string date, SelfAssessmentSnapshot? personalizationSnapshot = null);

    DailyChallenge GetChallengeForProgramDay(int programDay, SelfAssessmentSnapshot? personalizationSnapshot = null);

    void ApplyPersonalization(SelfAssessmentSnapshot snapshot);

    DailyChallenge AdvanceChallengeStepStatus(string date, StepType stepType);

    IReadOnlyList<string> ListCompletedDates();

    IReadOnlyList<DailyChallenge> ListChallengesBetween(string startDate, string endDate);

    IReadOnlyList<DailyChallenge> ListAllChallenges();

    int GetCompletedChallengesCount();
}
