using ImproveYourself.Backend.Application.Common.Interfaces;
using ImproveYourself.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ImproveYourself.Backend.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<DailyChallenge> DailyChallenges => Set<DailyChallenge>();
    public DbSet<ChallengeStep> ChallengeSteps => Set<ChallengeStep>();
    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<AnalyticsEvent> AnalyticsEvents => Set<AnalyticsEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}
