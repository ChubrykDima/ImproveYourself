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
    string DailyReason);

internal sealed record PersonalizedPracticeTemplate(
    string Subtitle,
    string Description,
    int DurationSeconds,
    string Tip);

internal sealed record PersonalizedSocialTemplate(
    string Description,
    string Tip);

public static class ChallengePersonalizer
{
    private static readonly IReadOnlyDictionary<SelfAssessmentFocus, IReadOnlyList<PersonalizedPracticeTemplate>> PracticeTemplates =
        new Dictionary<SelfAssessmentFocus, IReadOnlyList<PersonalizedPracticeTemplate>>
        {
            [SelfAssessmentFocus.ConversationStart] =
            [
                new(
                    "Голос перед стартом",
                    "Сделай 5 спокойных вдохов, затем вслух произнеси одну простую фразу для начала разговора.",
                    180,
                    "Говори чуть медленнее обычного и оставь паузу после фразы."),
                new(
                    "Разогрев контакта",
                    "2 минуты держи открытую осанку и проговори вслух три нейтральных вопроса по ситуации.",
                    120,
                    "Выбирай фразы, которые звучат естественно именно для тебя."),
            ],
            [SelfAssessmentFocus.OpinionExpression] =
            [
                new(
                    "Точка зрения",
                    "Запиши одну простую позицию на сегодня: Я выбираю... Затем произнеси её вслух спокойно.",
                    180,
                    "Достаточно короткой фразы без оправданий."),
                new(
                    "Спокойная формулировка",
                    "Сделай 4 дыхательных цикла и вслух закончи фразу: Мне ближе вариант..., потому что...",
                    180,
                    "Причина может быть простой, не нужно доказывать всё сразу."),
            ],
            [SelfAssessmentFocus.BodyCalm] =
            [
                new(
                    "Снижение напряжения",
                    "Сядь устойчиво, расслабь плечи и сделай 6 циклов: вдох 4 секунды, выдох 6 секунд.",
                    180,
                    "Длинный выдох помогает телу быстрее почувствовать безопасность."),
                new(
                    "Опора в теле",
                    "2 минуты стой ровно, почувствуй стопы и медленно расслабь челюсть, плечи и ладони.",
                    120,
                    "Возвращай внимание к стопам каждый раз, когда мысли ускоряются."),
            ],
            [SelfAssessmentFocus.EyeContact] =
            [
                new(
                    "Мягкий взгляд",
                    "2 минуты потренируй спокойный взгляд: смотри на точку на уровне глаз и дыши ровно.",
                    120,
                    "Взгляд не должен быть напряженным, можно мягко моргать."),
                new(
                    "Контакт без давления",
                    "Сделай 5 дыхательных циклов и представь короткий разговор с мягким зрительным контактом.",
                    180,
                    "Цель - не выиграть взглядом, а остаться спокойным."),
            ],
            [SelfAssessmentFocus.SocialAction] =
            [
                new(
                    "Мини-шаг",
                    "Выбери один маленький социальный шаг на день и проговори его вслух в формате: Сегодня я...",
                    120,
                    "Сделай шаг настолько маленьким, чтобы его трудно было отложить."),
                new(
                    "Готовность к действию",
                    "Сделай короткую разминку плеч и 5 спокойных вдохов, затем назови первый социальный шаг дня.",
                    180,
                    "Не усложняй: один шаг лучше идеального плана."),
            ],
        };

    private static readonly IReadOnlyDictionary<SelfAssessmentFocus, IReadOnlyList<PersonalizedSocialTemplate>> SocialTemplates =
        new Dictionary<SelfAssessmentFocus, IReadOnlyList<PersonalizedSocialTemplate>>
        {
            [SelfAssessmentFocus.ConversationStart] =
            [
                new(
                    "Начни один короткий разговор с простой фразы по ситуации: дорога, погода, очередь, работа или общая задача.",
                    "Твоя цель - не впечатлить, а спокойно открыть контакт."),
                new(
                    "Задай знакомому или незнакомому человеку один нейтральный вопрос и дождись ответа без спешки.",
                    "Вопрос должен быть коротким и конкретным."),
            ],
            [SelfAssessmentFocus.OpinionExpression] =
            [
                new(
                    "В одном бытовом выборе сегодня обозначь своё мнение вместо фразы Мне всё равно.",
                    "Короткого Я бы выбрал этот вариант достаточно."),
                new(
                    "Скажи одну мягкую позицию в разговоре: Мне кажется..., Я бы сделал..., Мне ближе...",
                    "Говори спокойно и не превращай мнение в спор."),
            ],
            [SelfAssessmentFocus.BodyCalm] =
            [
                new(
                    "Перед одним социальным действием сделай длинный выдох, расслабь плечи и только потом говори.",
                    "Сначала успокой тело, затем делай шаг."),
                new(
                    "Во время короткого разговора один раз осознанно почувствуй стопы и замедли речь.",
                    "Это тренировка спокойствия внутри контакта."),
            ],
            [SelfAssessmentFocus.EyeContact] =
            [
                new(
                    "Поздоровайся с одним человеком и удержи мягкий зрительный контакт 1-2 секунды.",
                    "Достаточно короткого спокойного взгляда, без напряжения."),
                new(
                    "В одном разговоре смотри на собеседника в начале и в конце своей фразы.",
                    "Можно переводить взгляд естественно, контакт не должен быть постоянным."),
            ],
            [SelfAssessmentFocus.SocialAction] =
            [
                new(
                    "Сделай один маленький социальный шаг до середины дня: приветствие, вопрос, благодарность или короткий комментарий.",
                    "Чем раньше закрыт шаг, тем меньше он разрастается в голове."),
                new(
                    "Выбери самый простой контакт дня и сделай его на 10% смелее обычного.",
                    "Например: сказать вслух вместо сообщения или добавить один вопрос."),
            ],
        };

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

        var title = GetFocusTitle(weakest.Focus);

        return new ChallengePersonalizationProfile(
            weakest.Focus,
            weakest.Score,
            title,
            $"Фокус дня: {title.ToLowerInvariant()}");
    }

    public static DailyChallenge Personalize(DailyChallenge challenge, SelfAssessmentSnapshot? snapshot)
    {
        var profile = CreateProfile(snapshot);

        if (profile is null)
        {
            return CloneChallenge(challenge);
        }

        var seed = BuildSeed(challenge.Date);
        var practice = PickBySeed(PracticeTemplates[profile.Focus], seed);
        var social = PickBySeed(SocialTemplates[profile.Focus], seed, offset: 1);

        return new DailyChallenge
        {
            Id = challenge.Id,
            Date = challenge.Date,
            Title = challenge.Title,
            Status = challenge.Status,
            CreatedAt = challenge.CreatedAt,
            QuoteText = challenge.QuoteText,
            QuoteAuthor = challenge.QuoteAuthor,
            QuoteNote = challenge.QuoteNote,
            Steps = challenge.Steps
                .Select(step => PersonalizeStep(step, practice, social))
                .OrderBy(step => step.SortOrder)
                .ToList(),
        };
    }

    private static ChallengeStep PersonalizeStep(
        ChallengeStep step,
        PersonalizedPracticeTemplate practice,
        PersonalizedSocialTemplate social)
    {
        var next = CloneStep(step);

        if (next.Type == StepType.Practice)
        {
            next.Title = "Утренняя практика";
            next.Subtitle = practice.Subtitle;
            next.Description = practice.Description;
            next.Tip = practice.Tip;
            next.DurationSeconds = practice.DurationSeconds;
        }

        if (next.Type == StepType.Social)
        {
            next.Title = "Челлендж дня";
            next.Description = social.Description;
            next.Tip = social.Tip;
        }

        return next;
    }

    private static string GetFocusTitle(SelfAssessmentFocus focus) => focus switch
    {
        SelfAssessmentFocus.ConversationStart => "первый короткий разговор",
        SelfAssessmentFocus.OpinionExpression => "спокойно обозначить мнение",
        SelfAssessmentFocus.BodyCalm => "успокоить тело перед действием",
        SelfAssessmentFocus.EyeContact => "мягкий зрительный контакт",
        SelfAssessmentFocus.SocialAction => "маленький социальный шаг",
        _ => "маленький социальный шаг",
    };

    private static int BuildSeed(string date)
    {
        var parsed = DateHelpers.TryParseIsoDate(date, out var dateOnly)
            ? dateOnly
            : DateOnly.FromDateTime(DateTime.Now);

        return parsed.Year + ((parsed.Month - 1) * 31) + parsed.Day;
    }

    private static T PickBySeed<T>(IReadOnlyList<T> templates, int seed, int offset = 0)
    {
        var index = Math.Abs(seed + offset) % templates.Count;

        return templates[index];
    }

    private static DailyChallenge CloneChallenge(DailyChallenge challenge) => new()
    {
        Id = challenge.Id,
        Date = challenge.Date,
        Title = challenge.Title,
        Status = challenge.Status,
        CreatedAt = challenge.CreatedAt,
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
    };

    private sealed record FocusCandidate(SelfAssessmentFocus Focus, int Score, int Priority);
}
