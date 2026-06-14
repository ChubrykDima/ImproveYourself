using ImproveYourself.Backend.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ImproveYourself.Backend.Application.Stats;

public record UserStatsDto(
    int TotalChallengesCompleted,
    int TotalStepsCompleted,
    int CurrentStreak);

public interface IStatsService
{
    Task<UserStatsDto> GetUserStatsAsync(string clientId);
}

public class StatsService : IStatsService
{
    private readonly IApplicationDbContext _context;

    public StatsService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserStatsDto> GetUserStatsAsync(string clientId)
    {
        // For MVP, we'll just return overall stats from the DB
        // In a real app, this would be filtered by user
        var completedChallenges = await _context.DailyChallenges
            .CountAsync(c => c.Status == Domain.Entities.ChallengeStatus.Completed);

        var completedSteps = await _context.ChallengeSteps
            .CountAsync(s => s.Status == Domain.Entities.StepStatus.Completed);

        return new UserStatsDto(completedChallenges, completedSteps, 0);
    }
}
