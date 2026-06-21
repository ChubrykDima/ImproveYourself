namespace ImproveYourself.Maui.Domain;

public enum SelfAssessmentKind
{
    Start,
    Final,
}

public sealed record SelfAssessmentQuestion(string Key, string Text);

public sealed class SelfAssessmentSnapshot
{
    public SelfAssessmentKind Kind { get; set; }

    public string CreatedAt { get; set; } = string.Empty;

    public int ConversationStartComfort { get; set; }

    public int OpinionExpressionEase { get; set; }

    public int BodyCalmUnderStress { get; set; }

    public int EyeContactEase { get; set; }

    public int SocialActionReadiness { get; set; }

    public double AverageScore => Scores.Count == 0 ? 0 : Scores.Average();

    public IReadOnlyList<int> Scores =>
    [
        ConversationStartComfort,
        OpinionExpressionEase,
        BodyCalmUnderStress,
        EyeContactEase,
        SocialActionReadiness,
    ];
}

public static class SelfAssessmentSurvey
{
    public static readonly IReadOnlyList<SelfAssessmentQuestion> Questions =
    [
        new("conversation", "Насколько комфортно первым начать разговор?"),
        new("opinion", "Насколько легко сказать своё мнение?"),
        new("body", "Насколько тело ощущается спокойным в стрессе?"),
        new("eyeContact", "Насколько легко удерживать спокойный зрительный контакт?"),
        new("action", "Насколько ты готов сделать небольшой социальный шаг, даже если неловко?"),
    ];

    public static SelfAssessmentSnapshot Create(SelfAssessmentKind kind, IReadOnlyList<int> scores)
    {
        var normalized = scores
            .Select(score => Math.Clamp(score, 1, 10))
            .Concat(Enumerable.Repeat(5, Questions.Count))
            .Take(Questions.Count)
            .ToArray();

        return new SelfAssessmentSnapshot
        {
            Kind = kind,
            CreatedAt = DateTime.UtcNow.ToString("o"),
            ConversationStartComfort = normalized[0],
            OpinionExpressionEase = normalized[1],
            BodyCalmUnderStress = normalized[2],
            EyeContactEase = normalized[3],
            SocialActionReadiness = normalized[4],
        };
    }
}
