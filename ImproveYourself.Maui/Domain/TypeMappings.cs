namespace ImproveYourself.Maui.Domain;

public static class TypeMappings
{
    public static string ToStorage(this StepType value) => value switch
    {
        StepType.Practice => "practice",
        StepType.Quote => "quote",
        StepType.Social => "social",
        _ => "practice",
    };

    public static string ToStorage(this StepStatus value) => value switch
    {
        StepStatus.NotStarted => "not_started",
        StepStatus.InProgress => "in_progress",
        StepStatus.Completed => "completed",
        _ => "not_started",
    };

    public static string ToStorage(this ChallengeStatus value) => value switch
    {
        ChallengeStatus.NotStarted => "not_started",
        ChallengeStatus.InProgress => "in_progress",
        ChallengeStatus.Completed => "completed",
        _ => "not_started",
    };

    public static StepType ToStepType(this string value) => value switch
    {
        "quote" => StepType.Quote,
        "social" => StepType.Social,
        _ => StepType.Practice,
    };

    public static StepStatus ToStepStatus(this string value) => value switch
    {
        "in_progress" => StepStatus.InProgress,
        "completed" => StepStatus.Completed,
        _ => StepStatus.NotStarted,
    };

    public static ChallengeStatus ToChallengeStatus(this string value) => value switch
    {
        "in_progress" => ChallengeStatus.InProgress,
        "completed" => ChallengeStatus.Completed,
        _ => ChallengeStatus.NotStarted,
    };

    public static string ToDisplayTitle(this StepStatus status) => status switch
    {
        StepStatus.NotStarted => "Не начато",
        StepStatus.InProgress => "В процессе",
        StepStatus.Completed => "Завершено",
        _ => "Не начато",
    };

    public static string ToActionTitle(this StepStatus status) => status switch
    {
        StepStatus.NotStarted => "Начать",
        StepStatus.InProgress => "Отметить выполненным",
        StepStatus.Completed => "Выполнено",
        _ => "Начать",
    };
}
