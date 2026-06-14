using ImproveYourself.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ImproveYourself.Backend.Infrastructure.Persistence.Configurations;

public class DailyChallengeConfiguration : IEntityTypeConfiguration<DailyChallenge>
{
    public void Configure(EntityTypeBuilder<DailyChallenge> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Date).IsRequired().HasMaxLength(10);
        builder.HasIndex(x => x.Date).IsUnique();

        builder.HasMany(x => x.Steps)
            .WithOne(x => x.DailyChallenge)
            .HasForeignKey(x => x.DailyChallengeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ChallengeStepConfiguration : IEntityTypeConfiguration<ChallengeStep>
{
    public void Configure(EntityTypeBuilder<ChallengeStep> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
    }
}

public class QuoteConfiguration : IEntityTypeConfiguration<Quote>
{
    public void Configure(EntityTypeBuilder<Quote> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Text).IsRequired();
        builder.Property(x => x.Author).IsRequired().HasMaxLength(200);
    }
}

public class AnalyticsEventConfiguration : IEntityTypeConfiguration<AnalyticsEvent>
{
    public void Configure(EntityTypeBuilder<AnalyticsEvent> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EventName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.ClientId).IsRequired().HasMaxLength(100);
    }
}
