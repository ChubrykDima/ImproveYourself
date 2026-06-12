using ImproveYourself.Maui.Domain;

namespace ImproveYourself.Maui.Persistence;

public interface IChallengeRepository
{
    void Initialize();

    DailyChallenge? GetChallengeByDate(string date);

    DailyChallenge GetOrCreateChallenge(string date);

    DailyChallenge AdvanceChallengeStepStatus(string date, StepType stepType);

    IReadOnlyList<string> ListCompletedDates();

    IReadOnlyList<DailyChallenge> ListChallengesBetween(string startDate, string endDate);

    int GetCompletedChallengesCount();
}
