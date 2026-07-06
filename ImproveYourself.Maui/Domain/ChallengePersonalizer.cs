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
    private static IReadOnlyDictionary<SelfAssessmentFocus, IReadOnlyList<PersonalizedPracticeTemplate>> PracticeTemplates =>
        ChallengeTemplateLanguage.Current switch
        {
            "ru" => RuPracticeTemplates,
            "de" => DePracticeTemplates,
            _ => EnPracticeTemplates,
        };

    private static IReadOnlyDictionary<SelfAssessmentFocus, IReadOnlyList<PersonalizedSocialTemplate>> SocialTemplates =>
        ChallengeTemplateLanguage.Current switch
        {
            "ru" => RuSocialTemplates,
            "de" => DeSocialTemplates,
            _ => EnSocialTemplates,
        };

    private static readonly IReadOnlyDictionary<SelfAssessmentFocus, IReadOnlyList<PersonalizedPracticeTemplate>> EnPracticeTemplates =
        new Dictionary<SelfAssessmentFocus, IReadOnlyList<PersonalizedPracticeTemplate>>
        {
            [SelfAssessmentFocus.ConversationStart] =
            [
                new("Voice before start", "Take 5 calm breaths, then say one simple phrase out loud to start a conversation.", 180, "Speak a little slower than usual and leave a pause after the phrase."),
                new("Contact warm-up", "For 2 minutes keep open posture and say three neutral situational questions out loud.", 120, "Choose phrases that sound natural for you."),
            ],
            [SelfAssessmentFocus.OpinionExpression] =
            [
                new("Point of view", "Write one simple position for today: I choose... Then say it out loud calmly.", 180, "A short phrase without justification is enough."),
                new("Calm wording", "Do 4 breathing cycles and finish out loud: I prefer option..., because...", 180, "The reason can be simple; you do not need to prove everything at once."),
            ],
            [SelfAssessmentFocus.BodyCalm] =
            [
                new("Release tension", "Sit steadily, relax your shoulders and do 6 cycles: inhale 4 seconds, exhale 6 seconds.", 180, "A long exhale helps your body feel safe faster."),
                new("Body anchor", "Stand straight for 2 minutes, feel your feet and slowly relax jaw, shoulders and palms.", 120, "Return attention to your feet whenever thoughts speed up."),
            ],
            [SelfAssessmentFocus.EyeContact] =
            [
                new("Soft gaze", "For 2 minutes practice a calm gaze: look at eye level and breathe evenly.", 120, "The gaze should not be tense; blinking softly is fine."),
                new("Contact without pressure", "Do 5 breathing cycles and imagine a short conversation with gentle eye contact.", 180, "The goal is not to win with your gaze, but to stay calm."),
            ],
            [SelfAssessmentFocus.SocialAction] =
            [
                new("Mini step", "Choose one small social step for the day and say it out loud: Today I...", 120, "Make the step so small it is hard to postpone."),
                new("Ready for action", "Do a short shoulder warm-up and 5 calm breaths, then name your first social step of the day.", 180, "Do not overcomplicate: one step beats a perfect plan."),
            ],
        };

    private static readonly IReadOnlyDictionary<SelfAssessmentFocus, IReadOnlyList<PersonalizedPracticeTemplate>> RuPracticeTemplates =
        new Dictionary<SelfAssessmentFocus, IReadOnlyList<PersonalizedPracticeTemplate>>
        {
            [SelfAssessmentFocus.ConversationStart] =
            [
                new("Голос перед стартом", "Сделай 5 спокойных вдохов, затем вслух произнеси одну простую фразу для начала разговора.", 180, "Говори чуть медленнее обычного и оставь паузу после фразы."),
                new("Разогрев контакта", "2 минуты держи открытую осанку и проговори вслух три нейтральных вопроса по ситуации.", 120, "Выбирай фразы, которые звучат естественно именно для тебя."),
            ],
            [SelfAssessmentFocus.OpinionExpression] =
            [
                new("Точка зрения", "Запиши одну простую позицию на сегодня: Я выбираю... Затем произнеси её вслух спокойно.", 180, "Достаточно короткой фразы без оправданий."),
                new("Спокойная формулировка", "Сделай 4 дыхательных цикла и вслух закончи фразу: Мне ближе вариант..., потому что...", 180, "Причина может быть простой, не нужно доказывать всё сразу."),
            ],
            [SelfAssessmentFocus.BodyCalm] =
            [
                new("Снижение напряжения", "Сядь устойчиво, расслабь плечи и сделай 6 циклов: вдох 4 секунды, выдох 6 секунд.", 180, "Длинный выдох помогает телу быстрее почувствовать безопасность."),
                new("Опора в теле", "2 минуты стой ровно, почувствуй стопы и медленно расслабь челюсть, плечи и ладони.", 120, "Возвращай внимание к стопам каждый раз, когда мысли ускоряются."),
            ],
            [SelfAssessmentFocus.EyeContact] =
            [
                new("Мягкий взгляд", "2 минуты потренируй спокойный взгляд: смотри на точку на уровне глаз и дыши ровно.", 120, "Взгляд не должен быть напряженным, можно мягко моргать."),
                new("Контакт без давления", "Сделай 5 дыхательных циклов и представь короткий разговор с мягким зрительным контактом.", 180, "Цель - не выиграть взглядом, а остаться спокойным."),
            ],
            [SelfAssessmentFocus.SocialAction] =
            [
                new("Мини-шаг", "Выбери один маленький социальный шаг на день и проговори его вслух в формате: Сегодня я...", 120, "Сделай шаг настолько маленьким, чтобы его трудно было отложить."),
                new("Готовность к действию", "Сделай короткую разминку плеч и 5 спокойных вдохов, затем назови первый социальный шаг дня.", 180, "Не усложняй: один шаг лучше идеального плана."),
            ],
        };

    private static readonly IReadOnlyDictionary<SelfAssessmentFocus, IReadOnlyList<PersonalizedPracticeTemplate>> DePracticeTemplates =
        new Dictionary<SelfAssessmentFocus, IReadOnlyList<PersonalizedPracticeTemplate>>
        {
            [SelfAssessmentFocus.ConversationStart] =
            [
                new("Stimme vor dem Start", "Atme 5-mal ruhig, dann sprich laut einen einfachen Satz, um ein Gespräch zu beginnen.", 180, "Sprich etwas langsamer als üblich und lasse eine Pause nach dem Satz."),
                new("Kontakt-Aufwärmen", "2 Minuten offene Haltung und laut drei neutrale situative Fragen sprechen.", 120, "Wähle Formulierungen, die für dich natürlich klingen."),
            ],
            [SelfAssessmentFocus.OpinionExpression] =
            [
                new("Standpunkt", "Notiere eine einfache Position für heute: Ich wähle... Dann sprich sie ruhig laut aus.", 180, "Ein kurzer Satz ohne Rechtfertigung reicht."),
                new("Ruhige Formulierung", "4 Atemzyklen und laut beenden: Mir liegt Option... näher, weil...", 180, "Der Grund kann einfach sein; du musst nicht alles sofort beweisen."),
            ],
            [SelfAssessmentFocus.BodyCalm] =
            [
                new("Spannung lösen", "Sitze stabil, entspanne die Schultern und mache 6 Zyklen: 4 Sek. ein, 6 Sek. aus.", 180, "Langes Ausatmen hilft dem Körper, sich schneller sicher zu fühlen."),
                new("Körperanker", "2 Minuten gerade stehen, Füße spüren und Kiefer, Schultern und Hände langsam entspannen.", 120, "Kehre zur Aufmerksamkeit auf die Füße zurück, wenn Gedanken rasen."),
            ],
            [SelfAssessmentFocus.EyeContact] =
            [
                new("Sanfter Blick", "2 Minuten ruhigen Blick üben: auf Augenhöhe schauen und gleichmäßig atmen.", 120, "Der Blick soll nicht angespannt sein; sanftes Blinzeln ist in Ordnung."),
                new("Kontakt ohne Druck", "5 Atemzyklen und ein kurzes Gespräch mit sanftem Augenkontakt vorstellen.", 180, "Ziel ist nicht, mit dem Blick zu gewinnen, sondern ruhig zu bleiben."),
            ],
            [SelfAssessmentFocus.SocialAction] =
            [
                new("Mini-Schritt", "Wähle einen kleinen sozialen Schritt für heute und sprich laut: Heute werde ich...", 120, "Mach den Schritt so klein, dass er schwer aufzuschieben ist."),
                new("Bereit für Aktion", "Kurze Schulter-Routine und 5 ruhige Atemzüge, dann den ersten sozialen Schritt des Tages nennen.", 180, "Nicht verkomplizieren: ein Schritt schlägt den perfekten Plan."),
            ],
        };

    private static readonly IReadOnlyDictionary<SelfAssessmentFocus, IReadOnlyList<PersonalizedSocialTemplate>> EnSocialTemplates =
        new Dictionary<SelfAssessmentFocus, IReadOnlyList<PersonalizedSocialTemplate>>
        {
            [SelfAssessmentFocus.ConversationStart] =
            [
                new("Start one short conversation with a simple situational phrase: commute, weather, queue, work or a shared task.", "Your goal is not to impress, but to calmly open contact."),
                new("Ask someone you know or a stranger one neutral question and wait for the answer without rushing.", "The question should be short and specific."),
            ],
            [SelfAssessmentFocus.OpinionExpression] =
            [
                new("In one everyday choice today, state your opinion instead of saying you do not care.", "A short I would choose this option is enough."),
                new("Say one soft position in a conversation: It seems to me..., I would..., I prefer...", "Speak calmly and do not turn opinion into an argument."),
            ],
            [SelfAssessmentFocus.BodyCalm] =
            [
                new("Before one social action, take a long exhale, relax your shoulders and only then speak.", "Calm the body first, then take the step."),
                new("During a short conversation, once consciously feel your feet and slow your speech.", "This trains calmness within contact."),
            ],
            [SelfAssessmentFocus.EyeContact] =
            [
                new("Greet one person and hold gentle eye contact for 1-2 seconds.", "A short calm look is enough, without tension."),
                new("In one conversation, look at the other person at the start and end of your sentence.", "You can look away naturally; constant contact is not required."),
            ],
            [SelfAssessmentFocus.SocialAction] =
            [
                new("Take one small social step before midday: greeting, question, thanks or a short comment.", "The earlier the step is done, the less it grows in your head."),
                new("Choose the simplest contact of the day and make it 10% bolder than usual.", "For example: speak out loud instead of messaging, or add one question."),
            ],
        };

    private static readonly IReadOnlyDictionary<SelfAssessmentFocus, IReadOnlyList<PersonalizedSocialTemplate>> RuSocialTemplates =
        new Dictionary<SelfAssessmentFocus, IReadOnlyList<PersonalizedSocialTemplate>>
        {
            [SelfAssessmentFocus.ConversationStart] =
            [
                new("Начни один короткий разговор с простой фразы по ситуации: дорога, погода, очередь, работа или общая задача.", "Твоя цель - не впечатлить, а спокойно открыть контакт."),
                new("Задай знакомому или незнакомому человеку один нейтральный вопрос и дождись ответа без спешки.", "Вопрос должен быть коротким и конкретным."),
            ],
            [SelfAssessmentFocus.OpinionExpression] =
            [
                new("В одном бытовом выборе сегодня обозначь своё мнение вместо фразы Мне всё равно.", "Короткого Я бы выбрал этот вариант достаточно."),
                new("Скажи одну мягкую позицию в разговоре: Мне кажется..., Я бы сделал..., Мне ближе...", "Говори спокойно и не превращай мнение в спор."),
            ],
            [SelfAssessmentFocus.BodyCalm] =
            [
                new("Перед одним социальным действием сделай длинный выдох, расслабь плечи и только потом говори.", "Сначала успокой тело, затем делай шаг."),
                new("Во время короткого разговора один раз осознанно почувствуй стопы и замедли речь.", "Это тренировка спокойствия внутри контакта."),
            ],
            [SelfAssessmentFocus.EyeContact] =
            [
                new("Поздоровайся с одним человеком и удержи мягкий зрительный контакт 1-2 секунды.", "Достаточно короткого спокойного взгляда, без напряжения."),
                new("В одном разговоре смотри на собеседника в начале и в конце своей фразы.", "Можно переводить взгляд естественно, контакт не должен быть постоянным."),
            ],
            [SelfAssessmentFocus.SocialAction] =
            [
                new("Сделай один маленький социальный шаг до середины дня: приветствие, вопрос, благодарность или короткий комментарий.", "Чем раньше закрыт шаг, тем меньше он разрастается в голове."),
                new("Выбери самый простой контакт дня и сделай его на 10% смелее обычного.", "Например: сказать вслух вместо сообщения или добавить один вопрос."),
            ],
        };

    private static readonly IReadOnlyDictionary<SelfAssessmentFocus, IReadOnlyList<PersonalizedSocialTemplate>> DeSocialTemplates =
        new Dictionary<SelfAssessmentFocus, IReadOnlyList<PersonalizedSocialTemplate>>
        {
            [SelfAssessmentFocus.ConversationStart] =
            [
                new("Beginne ein kurzes Gespräch mit einer einfachen Situationsphrase: Weg, Wetter, Warteschlange, Arbeit oder gemeinsame Aufgabe.", "Dein Ziel ist nicht zu beeindrucken, sondern ruhig Kontakt zu öffnen."),
                new("Stelle einer bekannten oder fremden Person eine neutrale Frage und warte ohne Eile auf die Antwort.", "Die Frage soll kurz und konkret sein."),
            ],
            [SelfAssessmentFocus.OpinionExpression] =
            [
                new("Bei einer alltäglichen Wahl heute gib deine Meinung statt zu sagen, es sei dir egal.", "Ein kurzes Ich würde diese Option wählen reicht."),
                new("Sag eine sanfte Position im Gespräch: Mir scheint..., Ich würde..., Mir liegt... näher.", "Sprich ruhig und mache aus Meinung keinen Streit."),
            ],
            [SelfAssessmentFocus.BodyCalm] =
            [
                new("Vor einer sozialen Handlung lang ausatmen, Schultern entspannen und erst dann sprechen.", "Erst den Körper beruhigen, dann den Schritt machen."),
                new("Während eines kurzen Gesprächs einmal bewusst die Füße spüren und die Sprache verlangsamen.", "Das trainiert Ruhe im Kontakt."),
            ],
            [SelfAssessmentFocus.EyeContact] =
            [
                new("Grüße eine Person und halte sanften Augenkontakt für 1-2 Sekunden.", "Ein kurzer ruhiger Blick reicht, ohne Anspannung."),
                new("In einem Gespräch schau am Anfang und Ende deines Satzes zum Gegenüber.", "Du kannst den Blick natürlich abwenden; ständiger Kontakt ist nicht nötig."),
            ],
            [SelfAssessmentFocus.SocialAction] =
            [
                new("Mache bis Mittag einen kleinen sozialen Schritt: Gruß, Frage, Dank oder kurzer Kommentar.", "Je früher der Schritt erledigt ist, desto weniger wächst er im Kopf."),
                new("Wähle den einfachsten Kontakt des Tages und mache ihn 10 % mutiger als üblich.", "Z.B. laut sprechen statt Nachricht, oder eine Frage hinzufügen."),
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
            string.Format(AppStrings.FocusDayReasonFormat, title.ToLowerInvariant()));
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
            UpdatedAt = challenge.UpdatedAt,
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
            next.Title = AppStrings.PracticeStep_Title;
            next.Subtitle = practice.Subtitle;
            next.Description = practice.Description;
            next.Tip = practice.Tip;
            next.DurationSeconds = practice.DurationSeconds;
        }

        if (next.Type == StepType.Social)
        {
            next.Title = AppStrings.SocialStep_Title;
            next.Description = social.Description;
            next.Tip = social.Tip;
        }

        return next;
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
        UpdatedAt = challenge.UpdatedAt,
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
