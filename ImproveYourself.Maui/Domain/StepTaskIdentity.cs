namespace ImproveYourself.Maui.Domain;

public static class StepTaskIdentity
{
    public static bool SameAssignment(ChallengeStep completed, ChallengeStep current) =>
        AssignmentKey(completed) == AssignmentKey(current);

    // IDs are shared by all translations. A personalization hint is stored separately
    // as the focus it came from, so a language change keeps the mark and another focus does not.
    public static string AssignmentKey(ChallengeStep step, SelfAssessmentFocus? focus = null)
    {
        var key = $"assignment:v1:{step.DailyChallengeId}:{step.Id}:{step.Type}";

        return focus is null ? key : $"{key}:focus:{focus}";
    }

    public static string VisibleKey(ChallengeStep step) => Key(step, step.Tip);

    // Upgrade only recognized course text, including its optional appended hint.
    // Unrecognized legacy assignments keep their old stamp instead of inheriting
    // completion merely because a newer course reused their ID.
    public static string? UpgradeLegacyCompletion(
        ChallengeStep step,
        IEnumerable<ChallengeStep> translations,
        IEnumerable<IReadOnlyDictionary<string, string>>? hintCatalogs = null)
    {
        if (string.IsNullOrEmpty(step.CompletionTask)
            || step.CompletionTask.StartsWith("assignment:", StringComparison.Ordinal))
        {
            return step.CompletionTask;
        }

        var catalogs = hintCatalogs ?? [];
        var recorded = step.CompletionTask.Split('\u001f');
        foreach (var translation in translations.Where(candidate => SameAssignment(step, candidate)))
        {
            var expected = VisibleKey(translation).Split('\u001f');
            if (recorded.Length != expected.Length
                || !recorded.Where((field, index) => index != 3).SequenceEqual(expected.Where((field, index) => index != 3)))
            {
                continue;
            }

            if (recorded[3] == expected[3])
            {
                return AssignmentKey(step);
            }

            var suffix = HintSuffix(expected[3], recorded[3]);
            var focus = ChallengePersonalizer.MatchHint(suffix, catalogs);
            if (focus is not null)
            {
                return AssignmentKey(step, focus);
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

        if (TryReadAssignment(step, step.CompletionTask, out var recordedFocus))
        {
            return recordedFocus is null || ChallengePersonalizer.MatchesFocus(appliedHint, recordedFocus.Value);
        }

        if (step.CompletionTask == VisibleKey(step))
        {
            return true;
        }

        var courseTip = RemoveAppliedHint(step.Tip, appliedHint);

        return courseTip is not null && step.CompletionTask == Key(step, courseTip);
    }

    private static string? HintSuffix(string courseTip, string recordedTip)
    {
        var separator = courseTip + "\n\n";

        return recordedTip.StartsWith(separator, StringComparison.Ordinal) && recordedTip.Length > separator.Length
            ? recordedTip[separator.Length..]
            : null;
    }

    private static bool TryReadAssignment(ChallengeStep step, string task, out SelfAssessmentFocus? focus)
    {
        focus = null;
        var bare = AssignmentKey(step);
        if (!task.StartsWith(bare, StringComparison.Ordinal))
        {
            return false;
        }

        if (task.Length == bare.Length)
        {
            return true;
        }

        const string marker = ":focus:";
        if (!task.StartsWith(bare + marker, StringComparison.Ordinal)
            || !Enum.TryParse(task[(bare.Length + marker.Length)..], out SelfAssessmentFocus parsed))
        {
            return false;
        }

        focus = parsed;

        return true;
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
