namespace ImproveYourself.Backend.Application.Analytics.Commands;

public record LogAnalyticsEventCommand(
    Guid Id,
    string EventName,
    string? EventData,
    DateTimeOffset Timestamp,
    string ClientId);
