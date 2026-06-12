namespace ImproveYourself.Maui.Domain;

public static class ProgressCalculator
{
    public static int CountCompletedSteps(IEnumerable<ChallengeStep> steps) =>
        steps.Count(step => step.Status == StepStatus.Completed);

    public static ChallengeStatus GetChallengeStatus(IReadOnlyList<ChallengeStep> steps)
    {
        if (steps.Count == 0)
        {
            return ChallengeStatus.NotStarted;
        }

        var completedSteps = CountCompletedSteps(steps);

        if (completedSteps == steps.Count)
        {
            return ChallengeStatus.Completed;
        }

        if (completedSteps > 0 || steps.Any(step => step.Status == StepStatus.InProgress))
        {
            return ChallengeStatus.InProgress;
        }

        return ChallengeStatus.NotStarted;
    }

    public static bool IsChallengeCompleted(IReadOnlyList<ChallengeStep> steps) =>
        GetChallengeStatus(steps) == ChallengeStatus.Completed;

    public static StepStatus GetNextStepStatus(StepStatus status) => status switch
    {
        StepStatus.NotStarted => StepStatus.InProgress,
        StepStatus.InProgress => StepStatus.Completed,
        _ => StepStatus.Completed,
    };

    public static StreakSnapshot CalculateStreakSnapshot(
        IEnumerable<string> completedDates,
        string? referenceDate = null)
    {
        var reference = referenceDate ?? DateHelpers.ToIsoDate(DateTime.Now);
        var sorted = NormalizeAndSortDates(completedDates);

        return new StreakSnapshot
        {
            CurrentStreakDays = CalculateCurrentStreak(sorted, reference),
            BestStreakDays = CalculateBestStreak(sorted),
            LastCompletedDate = sorted.Count > 0 ? sorted[^1] : null,
            UpdatedAt = DateTime.UtcNow.ToString("o"),
        };
    }

    public static MonthlyProgress CalculateMonthlyProgress(
        IEnumerable<string> completedDates,
        DateTime? referenceDate = null,
        int targetDays = DateHelpers.TargetMonthlyDays)
    {
        var reference = referenceDate ?? DateTime.Now;
        var monthKey = DateHelpers.MonthKeyFromDate(reference);
        var completedDays = NormalizeAndSortDates(completedDates)
            .Count(date => date.StartsWith(monthKey, StringComparison.Ordinal));

        var percent = targetDays <= 0
            ? 0
            : Math.Min(100, (int)Math.Round((double)completedDays / targetDays * 100, MidpointRounding.AwayFromZero));

        return new MonthlyProgress
        {
            CompletedDays = completedDays,
            TargetDays = targetDays,
            Percent = percent,
            RemainingDays = Math.Max(targetDays - completedDays, 0),
        };
    }

    public static WeeklyStats BuildWeeklyStats(
        IEnumerable<DailyChallenge> challenges,
        string? referenceDate = null)
    {
        var reference = DateHelpers.ParseIsoDate(referenceDate ?? DateHelpers.ToIsoDate(DateTime.Now));
        var challengeList = challenges.ToList();
        var lookup = challengeList.ToDictionary(challenge => challenge.Date, challenge => challenge);

        var days = Enumerable.Range(0, 7)
            .Select(index =>
            {
                var date = reference.AddDays(index - 6);
                var isoDate = date.ToString("yyyy-MM-dd");

                if (!lookup.TryGetValue(isoDate, out var challenge))
                {
                    return new WeeklyDayStat
                    {
                        Date = isoDate,
                        CompletedSteps = 0,
                        TotalSteps = 0,
                        IsCompleted = false,
                    };
                }

                var completedSteps = CountCompletedSteps(challenge.Steps);
                var totalSteps = challenge.Steps.Count;

                return new WeeklyDayStat
                {
                    Date = isoDate,
                    CompletedSteps = completedSteps,
                    TotalSteps = totalSteps,
                    IsCompleted = totalSteps > 0 && completedSteps == totalSteps,
                };
            })
            .ToList();

        var totalCompletedDays = days.Count(day => day.IsCompleted);
        var categoryBreakdown = new Dictionary<StepType, int>
        {
            [StepType.Practice] = 0,
            [StepType.Social] = 0,
        };

        foreach (var challenge in challengeList)
        {
            foreach (var step in challenge.Steps.Where(step => step.Status == StepStatus.Completed))
            {
                if (categoryBreakdown.TryGetValue(step.Type, out var value))
                {
                    categoryBreakdown[step.Type] = value + 1;
                }
            }
        }

        return new WeeklyStats
        {
            Days = days,
            TotalCompletedDays = totalCompletedDays,
            CompletionRate = (int)Math.Round(totalCompletedDays / 7d * 100, MidpointRounding.AwayFromZero),
            CategoryBreakdown = categoryBreakdown,
        };
    }

    public static int CalculateRollingCompletionRate(
        IEnumerable<string> completedDates,
        int periodDays = DateHelpers.TargetMonthlyDays,
        string? referenceDate = null)
    {
        var reference = DateHelpers.ParseIsoDate(referenceDate ?? DateHelpers.ToIsoDate(DateTime.Now));
        var periodStart = reference.AddDays(-(periodDays - 1));

        var completedInRange = NormalizeAndSortDates(completedDates)
            .Select(DateHelpers.ParseIsoDate)
            .Count(date => date >= periodStart && date <= reference);

        return (int)Math.Round(completedInRange / (double)periodDays * 100, MidpointRounding.AwayFromZero);
    }

    public static IReadOnlyList<string> ListCalendarDaysForMonth(DateTime referenceDate)
    {
        var start = new DateOnly(referenceDate.Year, referenceDate.Month, 1);
        var end = start.AddMonths(1).AddDays(-1);

        var days = new List<string>();
        var cursor = start;

        while (cursor <= end)
        {
            days.Add(cursor.ToString("yyyy-MM-dd"));
            cursor = cursor.AddDays(1);
        }

        return days;
    }

    private static List<string> NormalizeAndSortDates(IEnumerable<string> completedDates) =>
        completedDates
            .Where(date => !string.IsNullOrWhiteSpace(date))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(date => date, StringComparer.Ordinal)
            .ToList();

    private static int CalculateBestStreak(IReadOnlyList<string> completedDates)
    {
        if (completedDates.Count == 0)
        {
            return 0;
        }

        var best = 1;
        var running = 1;

        for (var index = 1; index < completedDates.Count; index += 1)
        {
            var currentDate = DateHelpers.ParseIsoDate(completedDates[index]);
            var previousDate = DateHelpers.ParseIsoDate(completedDates[index - 1]);
            var diff = currentDate.DayNumber - previousDate.DayNumber;

            if (diff == 1)
            {
                running += 1;
                best = Math.Max(best, running);
                continue;
            }

            running = 1;
        }

        return best;
    }

    private static int CalculateCurrentStreak(IReadOnlyList<string> completedDates, string referenceDate)
    {
        if (completedDates.Count == 0)
        {
            return 0;
        }

        var lastCompletedDate = completedDates[^1];
        var reference = DateHelpers.ParseIsoDate(referenceDate);
        var lastCompleted = DateHelpers.ParseIsoDate(lastCompletedDate);

        if (lastCompleted > reference)
        {
            return 0;
        }

        var gapFromReference = reference.DayNumber - lastCompleted.DayNumber;

        if (gapFromReference > 1)
        {
            return 0;
        }

        var streak = 1;
        var cursor = completedDates.Count - 1;

        while (cursor > 0)
        {
            var current = DateHelpers.ParseIsoDate(completedDates[cursor]);
            var previous = DateHelpers.ParseIsoDate(completedDates[cursor - 1]);
            var diff = current.DayNumber - previous.DayNumber;

            if (diff != 1)
            {
                break;
            }

            streak += 1;
            cursor -= 1;
        }

        return streak;
    }
}
