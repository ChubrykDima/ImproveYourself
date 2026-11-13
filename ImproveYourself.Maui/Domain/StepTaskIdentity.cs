namespace ImproveYourself.Maui.Domain;

public static class StepTaskIdentity
{
    public static bool SameAssignment(ChallengeStep completed, ChallengeStep current) =>
        AssignmentKey(completed) == AssignmentKey(current);

    // IDs are shared by all translations. Hints do not change the assignment.
    public static string AssignmentKey(ChallengeStep step) =>
        $"assignment:v1:{step.DailyChallengeId}:{step.Id}:{step.Type}";

    public static string VisibleKey(ChallengeStep step) => Key(step, step.Tip);

    // Upgrade only recognized course text, including its optional appended hint.
    // Unrecognized legacy assignments keep their old stamp instead of inheriting
    // completion merely because a newer course reused their ID.
    public static string? UpgradeLegacyCompletion(ChallengeStep step, IEnumerable<ChallengeStep> translations)
    {
        if (string.IsNullOrEmpty(step.CompletionTask)
            || step.CompletionTask.StartsWith("assignment:", StringComparison.Ordinal))
        {
            return step.CompletionTask;
        }

        var recorded = step.CompletionTask.Split('\u001f');
        foreach (var translation in translations.Where(candidate => SameAssignment(step, candidate)))
        {
            var expected = VisibleKey(translation).Split('\u001f');
            if (recorded.Length == expected.Length
                && recorded.Where((field, index) => index != 3).SequenceEqual(expected.Where((field, index) => index != 3))
                && (recorded[3] == expected[3]
                    || recorded[3].StartsWith(expected[3] + "\n\n", StringComparison.Ordinal)))
            {
                return AssignmentKey(step);
            }
        }

        return step.CompletionTask;
    }

    public static bool CompletionBelongsTo(ChallengeStep step, string? appliedHint = null)
    {
        if (step.Status != StepStatus.Completed || string.IsNullOrEmpty(step.CompletionTask))
        {
            return true;
        }

        if (step.CompletionTask == AssignmentKey(step) || step.CompletionTask == VisibleKey(step))
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
