namespace ImproveYourself.Maui.Domain;

public readonly record struct StepProgressState(StepStatus Status, string? CompletedAt, string UpdatedAt);

public sealed class RemoteProgramStep
{
    public StepType Type { get; init; }

    public StepStatus Status { get; init; }

    public string? CompletedAt { get; init; }

    public string UpdatedAt { get; init; } = string.Empty;
}

public sealed class RemoteProgramDay
{
    public int ProgramDayNumber { get; init; }

    public string? CompletedAt { get; init; }

    public List<RemoteProgramStep> Steps { get; init; } = [];
}

public static class CloudProgress
{
    public static int StatusRank(StepStatus status) => status switch
    {
        StepStatus.Completed => 2,
        StepStatus.InProgress => 1,
        _ => 0,
    };

    public static StepProgressState MergeStep(StepProgressState local, StepProgressState remote, bool authoritative)
    {
        if (authoritative || StatusRank(remote.Status) > StatusRank(local.Status))
        {
            return remote;
        }

        if (remote.Status == StepStatus.Completed
            && local.Status == StepStatus.Completed
            && string.IsNullOrWhiteSpace(local.CompletedAt)
            && !string.IsNullOrWhiteSpace(remote.CompletedAt))
        {
            return local with { CompletedAt = remote.CompletedAt };
        }

        return local;
    }

    public static string? MergeChallengeCompletedAt(
        ChallengeStatus mergedStatus,
        string? localCompletedAt,
        string? remoteCompletedAt,
        bool authoritative)
    {
        if (mergedStatus != ChallengeStatus.Completed)
        {
            return null;
        }

        if (!authoritative && !string.IsNullOrWhiteSpace(localCompletedAt))
        {
            return localCompletedAt;
        }

        if (!string.IsNullOrWhiteSpace(remoteCompletedAt))
        {
            return remoteCompletedAt;
        }

        return string.IsNullOrWhiteSpace(localCompletedAt) ? null : localCompletedAt;
    }

    public static int ResolveProgramDay(IEnumerable<DailyChallenge> challenges, string? localToday = null)
    {
        var today = localToday ?? DateHelpers.LocalTodayIsoDate;
        var byDay = challenges
            .Where(challenge => challenge.ProgramDayNumber is >= 1 and <= DateHelpers.TargetMonthlyDays)
            .GroupBy(challenge => challenge.ProgramDayNumber)
            .ToDictionary(group => group.Key, group => group.Last());

        var lastTouched = 0;

        for (var day = 1; day <= DateHelpers.TargetMonthlyDays; day++)
        {
            if (byDay.TryGetValue(day, out var challenge) && challenge.Status != ChallengeStatus.NotStarted)
            {
                lastTouched = day;
            }
        }

        if (lastTouched == 0)
        {
            return 1;
        }

        var current = byDay[lastTouched];

        if (current.Status == ChallengeStatus.Completed
            && lastTouched < DateHelpers.TargetMonthlyDays
            && CompletedBeforeToday(current.CompletedAt, today))
        {
            return lastTouched + 1;
        }

        return lastTouched;
    }

    private static bool CompletedBeforeToday(string? completedAt, string localToday)
    {
        if (!DateTimeOffset.TryParse(completedAt, out var timestamp))
        {
            return true;
        }

        return string.CompareOrdinal(DateHelpers.ToLocalIsoDate(timestamp), localToday) < 0;
    }
}
