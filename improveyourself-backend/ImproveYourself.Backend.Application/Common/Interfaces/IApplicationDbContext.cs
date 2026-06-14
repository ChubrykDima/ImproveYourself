using ImproveYourself.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ImproveYourself.Backend.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<DailyChallenge> DailyChallenges { get; }
    DbSet<ChallengeStep> ChallengeSteps { get; }
    DbSet<Quote> Quotes { get; }
    DbSet<AnalyticsEvent> AnalyticsEvents { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
