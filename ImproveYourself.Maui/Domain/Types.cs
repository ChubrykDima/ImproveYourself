namespace ImproveYourself.Maui.Domain;

public enum StepType
{
    Practice,
    Quote,
    Social,
}

public enum StepStatus
{
    NotStarted,
    InProgress,
    Completed,
}

public enum ChallengeStatus
{
    NotStarted,
    InProgress,
    Completed,
}

public sealed class ChallengeStep
{
    public string Id { get; set; } = string.Empty;

    public string DailyChallengeId { get; set; } = string.Empty;

    public StepType Type { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Subtitle { get; set; }

    public string Description { get; set; } = string.Empty;

    public string? Tip { get; set; }

    public int? DurationSeconds { get; set; }

    public string? QuoteText { get; set; }

    public string? QuoteAuthor { get; set; }

    public int SortOrder { get; set; }

    public StepStatus Status { get; set; }

    public string? CompletedAt { get; set; }
}

public sealed class DailyChallenge
{
    public string Id { get; set; } = string.Empty;

    public string Date { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public ChallengeStatus Status { get; set; }

    public string CreatedAt { get; set; } = string.Empty;

    public List<ChallengeStep> Steps { get; set; } = [];
}

public sealed class StreakSnapshot
{
    public int CurrentStreakDays { get; set; }

    public int BestStreakDays { get; set; }

    public string? LastCompletedDate { get; set; }

    public string UpdatedAt { get; set; } = string.Empty;

    public static StreakSnapshot Empty => new()
    {
        CurrentStreakDays = 0,
        BestStreakDays = 0,
        LastCompletedDate = null,
        UpdatedAt = DateTime.UtcNow.ToString("o"),
    };
}

public sealed class MonthlyProgress
{
    public int CompletedDays { get; set; }

    public int TargetDays { get; set; }

    public int Percent { get; set; }

    public int RemainingDays { get; set; }

    public static MonthlyProgress Empty => new()
    {
        CompletedDays = 0,
        TargetDays = DateHelpers.TargetMonthlyDays,
        Percent = 0,
        RemainingDays = DateHelpers.TargetMonthlyDays,
    };
}

public sealed class WeeklyDayStat
{
    public string Date { get; set; } = string.Empty;

    public int CompletedSteps { get; set; }

    public bool IsCompleted { get; set; }
}

public sealed class WeeklyStats
{
    public List<WeeklyDayStat> Days { get; set; } = [];

    public int TotalCompletedDays { get; set; }

    public int CompletionRate { get; set; }

    public Dictionary<StepType, int> CategoryBreakdown { get; set; } = new()
    {
        [StepType.Practice] = 0,
        [StepType.Quote] = 0,
        [StepType.Social] = 0,
    };

    public static WeeklyStats Empty => new();
}
