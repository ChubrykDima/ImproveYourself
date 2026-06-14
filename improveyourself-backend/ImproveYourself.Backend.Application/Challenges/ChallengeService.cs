using ImproveYourself.Backend.Application.Challenges.Queries;
using ImproveYourself.Backend.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ImproveYourself.Backend.Application.Challenges;

public interface IChallengeService
{
    Task<List<DailyChallengeDto>> GetChallengesAsync(string? startDate, string? endDate);
    Task SyncChallengesAsync(List<Commands.SyncDailyChallengeDto> challenges);
}

public class ChallengeService : IChallengeService
{
    private readonly IApplicationDbContext _context;

    public ChallengeService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DailyChallengeDto>> GetChallengesAsync(string? startDate, string? endDate)
    {
        var query = _context.DailyChallenges.Include(c => c.Steps).AsQueryable();

        if (!string.IsNullOrEmpty(startDate))
        {
            query = query.Where(c => c.Date.CompareTo(startDate) >= 0);
        }

        if (!string.IsNullOrEmpty(endDate))
        {
            query = query.Where(c => c.Date.CompareTo(endDate) <= 0);
        }

        var entities = await query.ToListAsync();

        return entities.Select(c => new DailyChallengeDto(
            c.Id,
            c.Date,
            c.Title,
            c.Status,
            c.CreatedAt,
            c.QuoteText,
            c.QuoteAuthor,
            c.UpdatedAt,
            c.Steps.Select(s => new ChallengeStepDto(
                s.Id,
                s.Type,
                s.Title,
                s.Subtitle,
                s.Description,
                s.Tip,
                s.DurationSeconds,
                s.QuoteText,
                s.QuoteAuthor,
                s.SortOrder,
                s.Status,
                s.CompletedAt,
                s.UpdatedAt)).ToList())).ToList();
    }

    public async Task SyncChallengesAsync(List<Commands.SyncDailyChallengeDto> challenges)
    {
        foreach (var dto in challenges)
        {
            var existing = await _context.DailyChallenges
                .Include(c => c.Steps)
                .FirstOrDefaultAsync(c => c.Id == dto.Id);

            if (existing == null)
            {
                var challenge = new Domain.Entities.DailyChallenge
                {
                    Id = dto.Id,
                    Date = dto.Date,
                    Title = dto.Title,
                    Status = dto.Status,
                    CreatedAt = dto.CreatedAt,
                    QuoteText = dto.QuoteText,
                    QuoteAuthor = dto.QuoteAuthor,
                    UpdatedAt = dto.UpdatedAt,
                    Steps = dto.Steps.Select(s => new Domain.Entities.ChallengeStep
                    {
                        Id = s.Id,
                        Type = s.Type,
                        Title = s.Title,
                        Subtitle = s.Subtitle,
                        Description = s.Description,
                        Tip = s.Tip,
                        DurationSeconds = s.DurationSeconds,
                        QuoteText = s.QuoteText,
                        QuoteAuthor = s.QuoteAuthor,
                        SortOrder = s.SortOrder,
                        Status = s.Status,
                        CompletedAt = s.CompletedAt,
                        UpdatedAt = s.UpdatedAt
                    }).ToList()
                };
                _context.DailyChallenges.Add(challenge);
            }
            else
            {
                if (dto.UpdatedAt > existing.UpdatedAt)
                {
                    existing.Title = dto.Title;
                    existing.Status = dto.Status;
                    existing.QuoteText = dto.QuoteText;
                    existing.QuoteAuthor = dto.QuoteAuthor;
                    existing.UpdatedAt = dto.UpdatedAt;
                }

                foreach (var stepDto in dto.Steps)
                {
                    var existingStep = existing.Steps.FirstOrDefault(s => s.Id == stepDto.Id);
                    if (existingStep == null)
                    {
                        existing.Steps.Add(new Domain.Entities.ChallengeStep
                        {
                            Id = stepDto.Id,
                            Type = stepDto.Type,
                            Title = stepDto.Title,
                            Subtitle = stepDto.Subtitle,
                            Description = stepDto.Description,
                            Tip = stepDto.Tip,
                            DurationSeconds = stepDto.DurationSeconds,
                            QuoteText = stepDto.QuoteText,
                            QuoteAuthor = stepDto.QuoteAuthor,
                            SortOrder = stepDto.SortOrder,
                            Status = stepDto.Status,
                            CompletedAt = stepDto.CompletedAt,
                            UpdatedAt = stepDto.UpdatedAt
                        });
                    }
                    else if (stepDto.UpdatedAt > existingStep.UpdatedAt)
                    {
                        existingStep.Title = stepDto.Title;
                        existingStep.Subtitle = stepDto.Subtitle;
                        existingStep.Description = stepDto.Description;
                        existingStep.Tip = stepDto.Tip;
                        existingStep.DurationSeconds = stepDto.DurationSeconds;
                        existingStep.QuoteText = stepDto.QuoteText;
                        existingStep.QuoteAuthor = stepDto.QuoteAuthor;
                        existingStep.SortOrder = stepDto.SortOrder;
                        existingStep.Status = stepDto.Status;
                        existingStep.CompletedAt = stepDto.CompletedAt;
                        existingStep.UpdatedAt = stepDto.UpdatedAt;
                    }
                }
            }
        }

        await _context.SaveChangesAsync(CancellationToken.None);
    }
}
