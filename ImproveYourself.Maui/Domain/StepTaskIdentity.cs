namespace ImproveYourself.Maui.Domain;

public static class StepTaskIdentity
{
    public static bool SameAssignment(ChallengeStep completed, ChallengeStep current) =>
        VisibleKey(completed) == VisibleKey(current);

    public static string VisibleKey(ChallengeStep step) => Key(step, step.Tip);

    // A completion recorded before this key existed stays put. Once the key is stored,
    // the mark belongs to that text: another wording is shown unchecked, and the
    // original wording shows the mark again. appliedHint is the personalization
    // suffix currently appended to the tip, so a wording stamp can ignore it.
    public static bool CompletionBelongsTo(ChallengeStep step, string? appliedHint = null)
    {
        if (step.Status != StepStatus.Completed || string.IsNullOrEmpty(step.CompletionTask))
        {
            return true;
        }

        if (step.CompletionTask == VisibleKey(step))
        {
            return true;
        }

        var courseTip = RemoveAppliedHint(step.Tip, appliedHint);

        return courseTip is not null && step.CompletionTask == Key(step, courseTip);
    }

    private static string? RemoveAppliedHint(string? tip, string? hint)
    {
        if (string.IsNullOrEmpty(hint) || string.IsNullOrEmpty(tip))
        {
            return null;
        }

        if (string.Equals(tip, hint, StringComparison.Ordinal))
        {
            return string.Empty;
        }

        var suffix = "\n\n" + hint;

        return tip.EndsWith(suffix, StringComparison.Ordinal)
            ? tip[..^suffix.Length]
            : null;
    }

    private static string Key(ChallengeStep step, string? tip) =>
        string.Join(
            '\u001f',
            Text(step.Title),
            Text(step.Subtitle),
            Text(step.Description),
            Text(tip),
            Text(step.QuoteText),
            Text(step.QuoteAuthor),
            Text(step.QuoteNote));

    private static string Text(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
}
