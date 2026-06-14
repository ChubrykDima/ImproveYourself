using FluentValidation;
using ImproveYourself.Backend.Application.Challenges.Commands;

namespace ImproveYourself.Backend.Application.Challenges.Commands;

public class SyncChallengesCommandValidator : AbstractValidator<SyncChallengesCommand>
{
    public SyncChallengesCommandValidator()
    {
        RuleFor(x => x.Challenges).NotEmpty();
        RuleForEach(x => x.Challenges).SetValidator(new SyncDailyChallengeDtoValidator());
    }
}

public class SyncDailyChallengeDtoValidator : AbstractValidator<SyncDailyChallengeDto>
{
    public SyncDailyChallengeDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Date).NotEmpty().Matches(@"^\d{4}-\d{2}-\d{2}$");
        RuleFor(x => x.Title).NotEmpty();
    }
}
