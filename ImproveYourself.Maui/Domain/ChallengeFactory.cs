using System.Globalization;

namespace ImproveYourself.Maui.Domain;

internal sealed record PracticeTemplate(
    string Title,
    string Subtitle,
    string Description,
    int DurationSeconds,
    string Tip);

internal sealed record QuoteTemplate(string Text, string Author, string Note);

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
        new(
            "Сначала назови себе свою роль, потом подтверди её самым неудобным делом дня.",
            "В духе Эпиктета",
            "Идентичность становится реальной, когда подтверждается действием, а не намерением."),
        new(
            "Не каждую мысль нужно слушаться: иногда она просто усталая версия вчерашнего страха.",
            "В духе Эпиктета",
            "Отдели факт от автоматической мысли и проверь, что реально происходит сейчас."),
        new(
            "Мысли становятся средой жизни: отсеки шум, оставь одну честную мысль и поступок.",
            "В духе Марка Аврелия",
            "Сократи внутренний шум до одного ясного вывода и одного шага на сегодня."),
        new(
            "Трудность полезна не сама по себе, а тем, как она тренирует выдержку.",
            "В духе Сенеки",
            "Сложность не всегда благо, но она показывает, где можно укрепить устойчивость."),
        new(
            "Даже маленькое действие меняет день; особенно то, которое ты решил не откладывать.",
            "В духе Уильяма Джеймса",
            "Маленький завершённый шаг обычно лечит бессилие лучше долгих размышлений."),
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
        var createdAt = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
        var practice = PickBySeed(PracticeTemplates, seed);
        var quote = PickBySeed(QuoteTemplates, seed, offset: 1);
        var social = PickBySeed(SocialTemplates, seed, offset: 2);

        return new DailyChallenge
        {
            Id = challengeId,
            Date = date,
            Title = "Твой ежедневный вызов",
            Status = ChallengeStatus.NotStarted,
            CreatedAt = createdAt,
            UpdatedAt = createdAt,
            QuoteText = quote.Text,
            QuoteAuthor = quote.Author,
            QuoteNote = quote.Note,
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
                    UpdatedAt = createdAt,
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
                    UpdatedAt = createdAt,
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
