using ImproveYourself.Backend.Domain.Entities;

namespace ImproveYourself.Backend.Application.Challenges.Queries;

public record ChallengeStepDto(
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

public record DailyChallengeDto(
    Guid Id,
    string Date,
    string Title,
    ChallengeStatus Status,
    DateTimeOffset CreatedAt,
    string? QuoteText,
    string? QuoteAuthor,
    DateTimeOffset UpdatedAt,
    List<ChallengeStepDto> Steps);
