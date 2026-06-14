using ImproveYourself.Backend.Application.Challenges;
using ImproveYourself.Backend.Application.Challenges.Commands;
using ImproveYourself.Backend.Domain.Entities;
using ImproveYourself.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ImproveYourself.Backend.Tests;

public class ChallengeServiceTests
{
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public ChallengeServiceTests()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task SyncChallengesAsync_ShouldAddNewChallenge()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var service = new ChallengeService(context);
        var challengeId = Guid.NewGuid();
        var challenges = new List<SyncDailyChallengeDto>
        {
            new SyncDailyChallengeDto(challengeId, "2026-06-14", "Title", ChallengeStatus.NotStarted, DateTimeOffset.UtcNow, null, null, DateTimeOffset.UtcNow, new List<SyncChallengeStepDto>())
        };

        // Act
        await service.SyncChallengesAsync(challenges);

        // Assert
        var stored = await context.DailyChallenges.FindAsync(challengeId);
        Assert.NotNull(stored);
        Assert.Equal("Title", stored.Title);
    }

    [Fact]
    public async Task SyncChallengesAsync_ShouldUpdateExistingChallenge_IfNewer()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var challengeId = Guid.NewGuid();
        var initialUpdate = DateTimeOffset.UtcNow.AddMinutes(-10);
        var newerUpdate = DateTimeOffset.UtcNow;

        context.DailyChallenges.Add(new DailyChallenge
        {
            Id = challengeId,
            Date = "2026-06-14",
            Title = "Old Title",
            Status = ChallengeStatus.NotStarted,
            CreatedAt = initialUpdate,
            UpdatedAt = initialUpdate
        });
        await context.SaveChangesAsync();

        var service = new ChallengeService(context);
        var challenges = new List<SyncDailyChallengeDto>
        {
            new SyncDailyChallengeDto(challengeId, "2026-06-14", "New Title", ChallengeStatus.InProgress, initialUpdate, null, null, newerUpdate, new List<SyncChallengeStepDto>())
        };

        // Act
        await service.SyncChallengesAsync(challenges);

        // Assert
        var stored = await context.DailyChallenges.FindAsync(challengeId);
        Assert.NotNull(stored);
        Assert.Equal("New Title", stored.Title);
        Assert.Equal(ChallengeStatus.InProgress, stored.Status);
    }

    [Fact]
    public async Task SyncChallengesAsync_ShouldNotUpdateExistingChallenge_IfOlder()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var challengeId = Guid.NewGuid();
        var initialUpdate = DateTimeOffset.UtcNow;
        var olderUpdate = DateTimeOffset.UtcNow.AddMinutes(-10);

        context.DailyChallenges.Add(new DailyChallenge
        {
            Id = challengeId,
            Date = "2026-06-14",
            Title = "New Title",
            Status = ChallengeStatus.InProgress,
            CreatedAt = olderUpdate,
            UpdatedAt = initialUpdate
        });
        await context.SaveChangesAsync();

        var service = new ChallengeService(context);
        var challenges = new List<SyncDailyChallengeDto>
        {
            new SyncDailyChallengeDto(challengeId, "2026-06-14", "Old Title", ChallengeStatus.NotStarted, olderUpdate, null, null, olderUpdate, new List<SyncChallengeStepDto>())
        };

        // Act
        await service.SyncChallengesAsync(challenges);

        // Assert
        var stored = await context.DailyChallenges.FindAsync(challengeId);
        Assert.NotNull(stored);
        Assert.Equal("New Title", stored.Title);
    }
}
