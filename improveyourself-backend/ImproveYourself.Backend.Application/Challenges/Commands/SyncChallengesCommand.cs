using ImproveYourself.Backend.Domain.Entities;

namespace ImproveYourself.Backend.Application.Challenges.Commands;

public record SyncChallengeStepDto(
    Guid Id,
    StepType Type,
    string Title,
    string? Subtitle,
    string Description,
    string? Tip,
    int? DurationSeconds,
    string? QuoteText,
    string? QuoteAuthor,
    int SortOrder,
    StepStatus Status,
    DateTimeOffset? CompletedAt,
    DateTimeOffset UpdatedAt);

public record SyncDailyChallengeDto(
    Guid Id,
    string Date,
    string Title,
    ChallengeStatus Status,
    DateTimeOffset CreatedAt,
    string? QuoteText,
    string? QuoteAuthor,
    DateTimeOffset UpdatedAt,
    List<SyncChallengeStepDto> Steps);

public record SyncChallengesCommand(List<SyncDailyChallengeDto> Challenges);
