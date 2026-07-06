using System.Globalization;
using ImproveYourself.Maui.Resources.Strings;

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
    private static IReadOnlyList<PracticeTemplate> PracticeTemplates =>
        ChallengeTemplateLanguage.Current switch
        {
            "ru" => RuPracticeTemplates,
            "de" => DePracticeTemplates,
            _ => EnPracticeTemplates,
        };

    private static IReadOnlyList<QuoteTemplate> QuoteTemplates =>
        ChallengeTemplateLanguage.Current switch
        {
            "ru" => RuQuoteTemplates,
            "de" => DeQuoteTemplates,
            _ => EnQuoteTemplates,
        };

    private static IReadOnlyList<SocialTemplate> SocialTemplates =>
        ChallengeTemplateLanguage.Current switch
        {
            "ru" => RuSocialTemplates,
            "de" => DeSocialTemplates,
            _ => EnSocialTemplates,
        };

    private static readonly IReadOnlyList<PracticeTemplate> EnPracticeTemplates =
    [
        new("Morning practice", "Contrast shower", "2 minutes of cool shower and focus on steady breathing.", 120, "Count slowly to 30 and keep your posture."),
        new("Morning practice", "Breath focus", "Do 3 cycles of 4-4-4-4: inhale, pause, exhale, pause.", 180, "Use a timer, but finish mindfully, not on autopilot."),
        new("Morning practice", "Mini workout", "Do 20 squats and 15 push-ups at a calm pace.", 300, "Clean form beats speed."),
        new("Morning practice", "Confidence stance", "Stand straight for 2 minutes, shoulders open, gaze forward.", 120, "Breathe slowly and do not look at your phone."),
        new("Morning practice", "Short meditation", "Spend 5 minutes observing your breath, returning without judgment.", 300, "If you drift, gently bring your focus back."),
    ];

    private static readonly IReadOnlyList<PracticeTemplate> RuPracticeTemplates =
    [
        new("Утренняя практика", "Контрастный душ", "2 минуты прохладного душа и фокус на ровном дыхании.", 120, "Считай медленно до 30 и держи осанку."),
        new("Утренняя практика", "Дыхательный фокус", "Сделай 3 цикла по 4-4-4-4: вдох, пауза, выдох, пауза.", 180, "Используй таймер, но завершай осознанно, а не на автомате."),
        new("Утренняя практика", "Мини-тренировка", "Сделай 20 приседаний и 15 отжиманий в спокойном темпе.", 300, "Лучше чистая техника, чем скорость."),
        new("Утренняя практика", "Поза уверенности", "2 минуты стой прямо, плечи раскрыты, взгляд вперед.", 120, "Дыши медленно и не смотри в телефон."),
        new("Утренняя практика", "Короткая медитация", "5 минут наблюдай дыхание, возвращай внимание без оценки.", 300, "Если отвлекся, просто мягко верни фокус."),
    ];

    private static readonly IReadOnlyList<PracticeTemplate> DePracticeTemplates =
    [
        new("Morgenübung", "Wechseldusche", "2 Minuten kühle Dusche und Fokus auf ruhiges Atmen.", 120, "Zähle langsam bis 30 und halte die Haltung."),
        new("Morgenübung", "Atemfokus", "Mache 3 Zyklen 4-4-4-4: Einatmen, Pause, Ausatmen, Pause.", 180, "Nutze einen Timer, aber beende bewusst, nicht automatisch."),
        new("Morgenübung", "Mini-Training", "Mache 20 Kniebeugen und 15 Liegestütze in ruhigem Tempo.", 300, "Saubere Technik ist wichtiger als Tempo."),
        new("Morgenübung", "Haltung der Zuversicht", "Stehe 2 Minuten aufrecht, Schultern offen, Blick nach vorn.", 120, "Atme langsam und schaue nicht aufs Handy."),
        new("Morgenübung", "Kurze Meditation", "5 Minuten den Atem beobachten, ohne zu bewerten zurückkehren.", 300, "Wenn du abschweifst, kehre sanft zurück."),
    ];

    private static readonly IReadOnlyList<QuoteTemplate> EnQuoteTemplates =
    [
        new("First name your role to yourself, then confirm it with the most uncomfortable task of the day.", "In the spirit of Epictetus", "Identity becomes real when confirmed by action, not intention."),
        new("Not every thought deserves obedience: sometimes it is just a tired version of yesterday's fear.", "In the spirit of Epictetus", "Separate fact from automatic thought and check what is really happening now."),
        new("Thoughts become the environment of life: cut the noise, keep one honest thought and one action.", "In the spirit of Marcus Aurelius", "Reduce inner noise to one clear conclusion and one step for today."),
        new("Difficulty is useful not in itself, but in how it trains endurance.", "In the spirit of Seneca", "Hardship is not always good, but it shows where you can build resilience."),
        new("Even a small action changes the day; especially the one you decided not to postpone.", "In the spirit of William James", "A small completed step usually heals helplessness better than long thinking."),
    ];

    private static readonly IReadOnlyList<QuoteTemplate> RuQuoteTemplates =
    [
        new("Сначала назови себе свою роль, потом подтверди её самым неудобным делом дня.", "В духе Эпиктета", "Идентичность становится реальной, когда подтверждается действием, а не намерением."),
        new("Не каждую мысль нужно слушаться: иногда она просто усталая версия вчерашнего страха.", "В духе Эпиктета", "Отдели факт от автоматической мысли и проверь, что реально происходит сейчас."),
        new("Мысли становятся средой жизни: отсеки шум, оставь одну честную мысль и поступок.", "В духе Марка Аврелия", "Сократи внутренний шум до одного ясного вывода и одного шага на сегодня."),
        new("Трудность полезна не сама по себе, а тем, как она тренирует выдержку.", "В духе Сенеки", "Сложность не всегда благо, но она показывает, где можно укрепить устойчивость."),
        new("Даже маленькое действие меняет день; особенно то, которое ты решил не откладывать.", "В духе Уильяма Джеймса", "Маленький завершённый шаг обычно лечит бессилие лучше долгих размышлений."),
    ];

    private static readonly IReadOnlyList<QuoteTemplate> DeQuoteTemplates =
    [
        new("Nenne dir zuerst deine Rolle, dann bestätige sie mit der unbequemsten Aufgabe des Tages.", "Im Geist Epiktets", "Identität wird real durch Handeln, nicht durch Absicht."),
        new("Nicht jeder Gedanke verdient Gehorsam: manchmal ist er nur eine müde Version von gestern.", "Im Geist Epiktets", "Trenne Fakt von automatischem Gedanken und prüfe, was jetzt wirklich passiert."),
        new("Gedanken werden zur Lebensumgebung: reduziere Lärm, behalte einen ehrlichen Gedanken und eine Tat.", "Im Geist Marc Aurels", "Reduziere inneren Lärm auf eine klare Schlussfolgerung und einen Schritt für heute."),
        new("Schwierigkeit ist nützlich nicht an sich, sondern weil sie Ausdauer trainiert.", "Im Geist Senecas", "Härte ist nicht immer gut, aber sie zeigt, wo du Widerstandskraft aufbauen kannst."),
        new("Selbst eine kleine Handlung verändert den Tag; besonders die, die du nicht aufschiebst.", "Im Geist William James'", "Ein kleiner abgeschlossener Schritt heilt Ohnmacht oft besser als langes Grübeln."),
    ];

    private static readonly IReadOnlyList<SocialTemplate> EnSocialTemplates =
    [
        new("Daily challenge", "Start a short dialogue with a stranger: 2-3 sentences.", "Begin with a simple question about the situation around you."),
        new("Daily challenge", "Give a colleague or acquaintance a sincere, specific compliment.", "The compliment should be concrete and honest."),
        new("Daily challenge", "Ask for feedback on one of your actions today.", "Listen without defending yourself; only clarify details."),
        new("Daily challenge", "Keep a conversation going for at least 5 minutes without your phone.", "Maintain eye contact and ask follow-up questions."),
        new("Daily challenge", "Be the first to greet three people during the day.", "Keep your voice steady, posture open, without rushing."),
    ];

    private static readonly IReadOnlyList<SocialTemplate> RuSocialTemplates =
    [
        new("Челлендж дня", "Заведи короткий диалог с незнакомым человеком: 2-3 фразы.", "Начни с простого вопроса по ситуации вокруг."),
        new("Челлендж дня", "Сделай комплимент коллеге или знакомому по делу.", "Комплимент должен быть конкретным и честным."),
        new("Челлендж дня", "Попроси обратную связь по одному своему действию сегодня.", "Слушай без оправданий, только уточняй детали."),
        new("Челлендж дня", "Поддержи разговор минимум 5 минут без телефона в руках.", "Поддерживай контакт глазами и задавай уточняющие вопросы."),
        new("Челлендж дня", "Поздоровайся первым с тремя людьми в течение дня.", "Голос ровный, осанка открытая, без спешки."),
    ];

    private static readonly IReadOnlyList<SocialTemplate> DeSocialTemplates =
    [
        new("Tagesherausforderung", "Führe ein kurzes Gespräch mit einer fremden Person: 2-3 Sätze.", "Beginne mit einer einfachen Frage zur Situation um dich herum."),
        new("Tagesherausforderung", "Mache einem Kollegen oder Bekannten ein ehrliches, konkretes Kompliment.", "Das Kompliment soll spezifisch und ehrlich sein."),
        new("Tagesherausforderung", "Bitte um Feedback zu einer deiner Handlungen heute.", "Höre zu ohne dich zu rechtfertigen; kläre nur Details."),
        new("Tagesherausforderung", "Halte ein Gespräch mindestens 5 Minuten ohne Handy.", "Halte Augenkontakt und stelle Nachfragen."),
        new("Tagesherausforderung", "Grüße als Erster drei Menschen am Tag.", "Stimme ruhig, Haltung offen, ohne Eile."),
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
            Title = AppStrings.DailyChallenge_DefaultTitle,
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
                    Title = AppStrings.PracticeStep_Title,
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
                    Title = AppStrings.SocialStep_Title,
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
