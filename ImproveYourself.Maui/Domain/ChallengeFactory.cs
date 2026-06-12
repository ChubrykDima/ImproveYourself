using System.Globalization;

namespace ImproveYourself.Maui.Domain;

internal sealed record PracticeTemplate(
    string Title,
    string Subtitle,
    string Description,
    int DurationSeconds,
    string Tip);

internal sealed record QuoteTemplate(string Text, string Author);

internal sealed record SocialTemplate(string Title, string Description, string Tip);

public static class ChallengeFactory
{
    private static readonly IReadOnlyList<PracticeTemplate> PracticeTemplates =
    [
        new(
            "Утренняя практика",
            "Контрастный душ",
            "2 минуты прохладного душа и фокус на ровном дыхании.",
            120,
            "Считай медленно до 30 и держи осанку."),
        new(
            "Утренняя практика",
            "Дыхательный фокус",
            "Сделай 3 цикла по 4-4-4-4: вдох, пауза, выдох, пауза.",
            180,
            "Используй таймер, но завершай осознанно, а не на автомате."),
        new(
            "Утренняя практика",
            "Мини-тренировка",
            "Сделай 20 приседаний и 15 отжиманий в спокойном темпе.",
            300,
            "Лучше чистая техника, чем скорость."),
        new(
            "Утренняя практика",
            "Поза уверенности",
            "2 минуты стой прямо, плечи раскрыты, взгляд вперед.",
            120,
            "Дыши медленно и не смотри в телефон."),
        new(
            "Утренняя практика",
            "Короткая медитация",
            "5 минут наблюдай дыхание, возвращай внимание без оценки.",
            300,
            "Если отвлекся, просто мягко верни фокус."),
    ];

    private static readonly IReadOnlyList<QuoteTemplate> QuoteTemplates =
    [
        new("Сначала скажи себе, кем хочешь быть, а затем делай, что должен.", "Эпиктет"),
        new("Нам мешают не сами вещи, а мнение о них.", "Эпиктет"),
        new("Счастье твоей жизни зависит от качества твоих мыслей.", "Марк Аврелий"),
        new("Трудности показывают, кто мы есть.", "Сенека"),
        new("Пока ты жив, продолжай становиться лучше.", "Сенека"),
    ];

    private static readonly IReadOnlyList<SocialTemplate> SocialTemplates =
    [
        new(
            "Челлендж дня",
            "Заведи короткий диалог с незнакомым человеком: 2-3 фразы.",
            "Начни с простого вопроса по ситуации вокруг."),
        new(
            "Челлендж дня",
            "Сделай комплимент коллеге или знакомому по делу.",
            "Комплимент должен быть конкретным и честным."),
        new(
            "Челлендж дня",
            "Попроси обратную связь по одному своему действию сегодня.",
            "Слушай без оправданий, только уточняй детали."),
        new(
            "Челлендж дня",
            "Поддержи разговор минимум 5 минут без телефона в руках.",
            "Поддерживай контакт глазами и задавай уточняющие вопросы."),
        new(
            "Челлендж дня",
            "Поздоровайся первым с тремя людьми в течение дня.",
            "Голос ровный, осанка открытая, без спешки."),
    ];

    public static DailyChallenge CreateDailyChallenge(string date)
    {
        var seed = BuildSeed(date);
        var challengeId = $"challenge-{date}";
        var practice = PickBySeed(PracticeTemplates, seed);
        var quote = PickBySeed(QuoteTemplates, seed, offset: 1);
        var social = PickBySeed(SocialTemplates, seed, offset: 2);

        return new DailyChallenge
        {
            Id = challengeId,
            Date = date,
            Title = "Твой ежедневный вызов",
            Status = ChallengeStatus.NotStarted,
            CreatedAt = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
            QuoteText = quote.Text,
            QuoteAuthor = quote.Author,
            Steps =
            [
                new ChallengeStep
                {
                    Id = $"{challengeId}-practice",
                    DailyChallengeId = challengeId,
                    Type = StepType.Practice,
                    Title = practice.Title,
                    Subtitle = practice.Subtitle,
                    Description = practice.Description,
                    Tip = practice.Tip,
                    DurationSeconds = practice.DurationSeconds,
                    SortOrder = 1,
                    Status = StepStatus.NotStarted,
                    CompletedAt = null,
                },
                new ChallengeStep
                {
                    Id = $"{challengeId}-social",
                    DailyChallengeId = challengeId,
                    Type = StepType.Social,
                    Title = social.Title,
                    Description = social.Description,
                    Tip = social.Tip,
                    SortOrder = 2,
                    Status = StepStatus.NotStarted,
                    CompletedAt = null,
                },
            ],
        };
    }

    private static int BuildSeed(string date)
    {
        var parsed = DateHelpers.ParseIsoDate(date);

        return parsed.Year + ((parsed.Month - 1) * 31) + parsed.Day;
    }

    private static T PickBySeed<T>(IReadOnlyList<T> templates, int seed, int offset = 0)
    {
        var index = Math.Abs(seed + offset) % templates.Count;

        return templates[index];
    }
}
