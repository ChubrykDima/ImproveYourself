namespace ImproveYourself.Backend.Domain.Entities;

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

public class ChallengeStep
{
    public Guid Id { get; set; }
    public Guid DailyChallengeId { get; set; }
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
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public DailyChallenge DailyChallenge { get; set; } = null!;
}

public class DailyChallenge
{
    public Guid Id { get; set; }
    public string Date { get; set; } = string.Empty; // format YYYY-MM-DD
    public string Title { get; set; } = string.Empty;
    public ChallengeStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? QuoteText { get; set; }
    public string? QuoteAuthor { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public List<ChallengeStep> Steps { get; set; } = [];
}

public class Quote
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? Category { get; set; }
}

public class AnalyticsEvent
{
    public Guid Id { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string? EventData { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string ClientId { get; set; } = string.Empty;
}
