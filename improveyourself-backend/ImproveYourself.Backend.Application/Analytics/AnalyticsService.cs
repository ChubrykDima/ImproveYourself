using ImproveYourself.Backend.Application.Analytics.Commands;
using ImproveYourself.Backend.Application.Common.Interfaces;
using ImproveYourself.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ImproveYourself.Backend.Application.Analytics;

public interface IAnalyticsService
{
    Task LogEventAsync(LogAnalyticsEventCommand command);
}

public class AnalyticsService : IAnalyticsService
{
    private readonly IApplicationDbContext _context;

    public AnalyticsService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogEventAsync(LogAnalyticsEventCommand command)
    {
        var existing = await _context.AnalyticsEvents.AnyAsync(e => e.Id == command.Id);
        if (existing) return;

        var @event = new AnalyticsEvent
        {
            Id = command.Id,
            EventName = command.EventName,
            EventData = command.EventData,
            Timestamp = command.Timestamp,
            ClientId = command.ClientId
        };

        _context.AnalyticsEvents.Add(@event);
        await _context.SaveChangesAsync(CancellationToken.None);
    }
}
