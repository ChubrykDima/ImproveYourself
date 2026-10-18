using ImproveYourself.Maui.Resources.Strings;

namespace ImproveYourself.Maui.Domain;

public enum SelfAssessmentFocus
{
    ConversationStart,
    OpinionExpression,
    BodyCalm,
    EyeContact,
    SocialAction,
}

public sealed record ChallengePersonalizationProfile(
    SelfAssessmentFocus Focus,
    int Score,
    string Title,
    string Tip);

public static class ChallengePersonalizer
{
    public static ChallengePersonalizationProfile? CreateProfile(SelfAssessmentSnapshot? snapshot)
    {
        if (snapshot is null)
        {
            return null;
        }

        var weakest = new[]
            {
                new FocusCandidate(SelfAssessmentFocus.ConversationStart, snapshot.ConversationStartComfort, 0),
                new FocusCandidate(SelfAssessmentFocus.OpinionExpression, snapshot.OpinionExpressionEase, 1),
                new FocusCandidate(SelfAssessmentFocus.BodyCalm, snapshot.BodyCalmUnderStress, 2),
                new FocusCandidate(SelfAssessmentFocus.EyeContact, snapshot.EyeContactEase, 3),
                new FocusCandidate(SelfAssessmentFocus.SocialAction, snapshot.SocialActionReadiness, 4),
            }
            .OrderBy(candidate => candidate.Score)
            .ThenBy(candidate => candidate.Priority)
            .First();

        return new ChallengePersonalizationProfile(
            weakest.Focus,
            weakest.Score,
            GetFocusTitle(weakest.Focus),
            GetFocusTip(weakest.Focus));
    }

    public static string? GetDayFocus(DailyChallenge challenge)
    {
        var steps = challenge.Steps.OrderBy(step => step.SortOrder).ToList();
        var focus = steps.FirstOrDefault(step => step.Type == StepType.Social)?.Subtitle;

        if (string.IsNullOrWhiteSpace(focus))
        {
            focus = steps.FirstOrDefault(step => step.Type == StepType.Practice)?.Subtitle;
        }

        return string.IsNullOrWhiteSpace(focus) ? null : focus.Trim();
    }

    // Program content must stay identical for a given day regardless of the date, the language
    // or the self-assessment, otherwise completion marks end up attached to different tasks.
    // Personalization therefore only appends a hint for the weakest self-assessment area.
    public static DailyChallenge Personalize(DailyChallenge challenge, SelfAssessmentSnapshot? snapshot)
    {
        var personalized = CloneChallenge(challenge);
        var profile = CreateProfile(snapshot);

        if (profile is null)
        {
            return personalized;
        }

        var socialStep = personalized.Steps.FirstOrDefault(step => step.Type == StepType.Social);

        if (socialStep is null)
        {
            return personalized;
        }

        var hint = string.Format(AppStrings.PersonalFocusTipFormat, profile.Title, profile.Tip);

        if (socialStep.Tip?.Contains(hint, StringComparison.Ordinal) == true)
        {
            return personalized;
        }

        socialStep.Tip = string.IsNullOrWhiteSpace(socialStep.Tip)
            ? hint
            : $"{socialStep.Tip}\n\n{hint}";

        return personalized;
    }

    private static string GetFocusTitle(SelfAssessmentFocus focus) => focus switch
    {
        SelfAssessmentFocus.ConversationStart => AppStrings.Focus_ConversationStart,
        SelfAssessmentFocus.OpinionExpression => AppStrings.Focus_OpinionExpression,
        SelfAssessmentFocus.BodyCalm => AppStrings.Focus_BodyCalm,
        SelfAssessmentFocus.EyeContact => AppStrings.Focus_EyeContact,
        SelfAssessmentFocus.SocialAction => AppStrings.Focus_SocialAction,
        _ => AppStrings.Focus_SocialAction,
    };

    private static string GetFocusTip(SelfAssessmentFocus focus) => focus switch
    {
        SelfAssessmentFocus.ConversationStart => AppStrings.FocusTip_ConversationStart,
        SelfAssessmentFocus.OpinionExpression => AppStrings.FocusTip_OpinionExpression,
        SelfAssessmentFocus.BodyCalm => AppStrings.FocusTip_BodyCalm,
        SelfAssessmentFocus.EyeContact => AppStrings.FocusTip_EyeContact,
        SelfAssessmentFocus.SocialAction => AppStrings.FocusTip_SocialAction,
        _ => AppStrings.FocusTip_SocialAction,
    };

    private static DailyChallenge CloneChallenge(DailyChallenge challenge) => new()
    {
        Id = challenge.Id,
        Date = challenge.Date,
        ProgramDayNumber = challenge.ProgramDayNumber,
        Title = challenge.Title,
        Status = challenge.Status,
        CreatedAt = challenge.CreatedAt,
        UpdatedAt = challenge.UpdatedAt,
        CompletedAt = challenge.CompletedAt,
        QuoteText = challenge.QuoteText,
        QuoteAuthor = challenge.QuoteAuthor,
        QuoteNote = challenge.QuoteNote,
        Steps = challenge.Steps.Select(CloneStep).ToList(),
    };

    private static ChallengeStep CloneStep(ChallengeStep step) => new()
    {
        Id = step.Id,
        DailyChallengeId = step.DailyChallengeId,
        Type = step.Type,
        Title = step.Title,
        Subtitle = step.Subtitle,
        Description = step.Description,
        Tip = step.Tip,
        DurationSeconds = step.DurationSeconds,
        QuoteText = step.QuoteText,
        QuoteAuthor = step.QuoteAuthor,
        QuoteNote = step.QuoteNote,
        SortOrder = step.SortOrder,
        Status = step.Status,
        CompletedAt = step.CompletedAt,
        UpdatedAt = step.UpdatedAt,
    };

    private sealed record FocusCandidate(SelfAssessmentFocus Focus, int Score, int Priority);
}
