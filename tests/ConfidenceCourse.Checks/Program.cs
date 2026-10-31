using System.Text.Json;
using System.Text.Json.Serialization;
using ImproveYourself.Maui.Domain;
using ImproveYourself.Maui.Resources.Strings;

static void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
options.Converters.Add(new JsonStringEnumConverter());

List<DailyChallenge> LoadCourse(string language)
{
    var days = JsonSerializer.Deserialize<List<DailyChallenge>>(
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, $"course.{language}.json")), options)!;
    var ordered = days.OrderBy(day => day.Date).ToList();

    for (var index = 0; index < ordered.Count; index++)
    {
        ordered[index].ProgramDayNumber = index + 1;
    }

    return ordered;
}

void LoadStrings(string language)
{
    var strings = JsonSerializer.Deserialize<Dictionary<string, string>>(
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, $"strings.{language}.json")))!;
    AppStrings.Load(strings, language);
    ChallengeTemplateLanguage.SetLanguage(language);
}

var labels = new Dictionary<string, (string Why, string Easier, string Harder, string Online, string Anxiety)>
{
    ["ru"] = ("Зачем:", "Проще:", "Сложнее (по желанию):", "Онлайн-вариант:", "от 0 до 10"),
    ["en"] = ("Why:", "Easier:", "Harder (optional):", "Online option:", "0 to 10"),
    ["de"] = ("Wozu:", "Leichter:", "Schwerer (optional):", "Online-Variante:", "0 bis 10"),
};
int[] daysWithOnlineOption = [8, 11, 13, 24];

var snapshot = new SelfAssessmentSnapshot
{
    ConversationStartComfort = 1, OpinionExpressionEase = 3, BodyCalmUnderStress = 3,
    EyeContactEase = 3, SocialActionReadiness = 3,
};

var courses = labels.Keys.ToDictionary(language => language, LoadCourse);
var referenceIds = courses["ru"].SelectMany(day => day.Steps.Select(step => step.Id).Prepend(day.Id)).ToList();

foreach (var (language, days) in courses)
{
    LoadStrings(language);
    var label = labels[language];

    Check(days.Count == 30, $"[{language}] The course must contain exactly 30 days.");
    Check(days.SelectMany(day => day.Steps).Select(step => step.Id).Distinct().Count() == 90,
        $"[{language}] Step IDs must remain unique.");
    Check(days.SelectMany(day => day.Steps.Select(step => step.Id).Prepend(day.Id)).SequenceEqual(referenceIds),
        $"[{language}] Day and step IDs must match the Russian program so progress survives a language switch.");
    Check(days.Select(day => day.Steps.Single(step => step.Type == StepType.Quote).QuoteNote)
              .Where(note => !string.IsNullOrWhiteSpace(note)).Distinct().Count() == 30,
        $"[{language}] Every day needs its own quote note.");

    foreach (var day in days)
    {
        var n = day.ProgramDayNumber;
        Check(day.Steps.Count == 3 && day.Steps.Select(step => step.Type).Distinct().Count() == 3,
            $"[{language}] Day {n} must retain the three-step data contract.");
        Check(day.Steps.All(step => step.DailyChallengeId == day.Id), $"[{language}] Day {n}: broken step parent.");

        var practice = day.Steps.Single(step => step.Type == StepType.Practice);
        var social = day.Steps.Single(step => step.Type == StepType.Social);

        Check(practice.Description.Contains(label.Why), $"[{language}] Day {n} lacks the purpose line.");
        Check(social.Description.Contains(label.Easier) && social.Description.Contains(label.Harder),
            $"[{language}] Day {n} lacks difficulty alternatives.");
        Check($"{practice.Tip} {social.Tip}".Contains(label.Anxiety),
            $"[{language}] Day {n} lacks the 0–10 anxiety rating.");
        Check(!string.IsNullOrWhiteSpace(ChallengePersonalizer.GetDayFocus(day)),
            $"[{language}] Day {n} has no focus to show.");

        if (daysWithOnlineOption.Contains(n))
        {
            Check(social.Description.Contains(label.Online), $"[{language}] Day {n} lacks an online option.");
        }

        var before = JsonSerializer.Serialize(day);
        var personalized = ChallengePersonalizer.Personalize(day, snapshot);
        Check(JsonSerializer.Serialize(day) == before, $"[{language}] Personalization mutated day {n}.");
        Check(!ReferenceEquals(day, personalized) && !ReferenceEquals(day.Steps[0], personalized.Steps[0]),
            $"[{language}] Personalization must preserve clone isolation.");

        foreach (var (original, result) in day.Steps.Zip(personalized.Steps))
        {
            Check(original.Id == result.Id && original.Title == result.Title
                  && original.Subtitle == result.Subtitle && original.Description == result.Description,
                $"[{language}] Personalization replaced the content of day {n}.");
        }

        var personalizedTip = personalized.Steps.Single(step => step.Type == StepType.Social).Tip!;
        Check(personalizedTip.StartsWith(social.Tip!, StringComparison.Ordinal)
              && personalizedTip.Contains(AppStrings.Focus_ConversationStart),
            $"[{language}] Day {n} must keep its tip and append the weakest-area hint.");
        Check(JsonSerializer.Serialize(ChallengePersonalizer.Personalize(personalized, snapshot))
              == JsonSerializer.Serialize(personalized),
            $"[{language}] Personalization must be idempotent.");
        Check(JsonSerializer.Serialize(ChallengePersonalizer.Personalize(day, null)) == before,
            $"[{language}] Without a self-assessment the day must stay untouched.");
    }
}

// Completion metadata must survive personalization.
LoadStrings("ru");
var completed = courses["ru"][0];
completed.Status = ChallengeStatus.Completed;
completed.CompletedAt = "2026-10-03T12:00:00Z";
foreach (var step in completed.Steps)
{
    step.Status = StepStatus.Completed;
    step.CompletedAt = completed.CompletedAt;
}
var personalizedCompleted = ChallengePersonalizer.Personalize(completed, snapshot);
Check(personalizedCompleted.Status == ChallengeStatus.Completed
      && personalizedCompleted.CompletedAt == completed.CompletedAt
      && personalizedCompleted.Steps.All(step => step.Status == StepStatus.Completed && step.CompletedAt == completed.CompletedAt),
    "Completion metadata changed.");

// Factory fallback: deterministic and not limited to a handful of combinations.
static string FactoryContent(string date)
{
    var day = ChallengeFactory.CreateDailyChallenge(date);
    return string.Join("|", day.Steps.OrderBy(step => step.SortOrder)
        .Select(step => $"{step.Title}/{step.Description}/{step.QuoteText}/{step.QuoteAuthor}"));
}

foreach (var language in labels.Keys)
{
    LoadStrings(language);
    Check(FactoryContent("2026-10-03") == FactoryContent("2026-10-03"),
        $"[{language}] Factory output must be deterministic.");
}
LoadStrings("ru");
var start = new DateOnly(2026, 1, 1);
var combinations = Enumerable.Range(0, 365)
    .Select(offset => FactoryContent(start.AddDays(offset).ToString("yyyy-MM-dd")))
    .Distinct()
    .Count();
Check(combinations > 5, $"Factory seeds collapsed to {combinations} combinations.");
Check(ChallengeFactory.StableSeed("practice", "2026-10-03") == ChallengeFactory.StableSeed("practice", "2026-10-03")
      && ChallengeFactory.StableSeed("practice", "2026-10-03") != ChallengeFactory.StableSeed("social", "2026-10-03"),
    "Seeds must be stable and independent per step.");

// Plural forms.
Check(AppStrings.Plural("DaysCount", 1) == "1 день", "ru: 1");
Check(AppStrings.Plural("DaysCount", 3) == "3 дня", "ru: 3");
Check(AppStrings.Plural("DaysCount", 5) == "5 дней", "ru: 5");
Check(AppStrings.Plural("DaysCount", 11) == "11 дней", "ru: 11");
Check(AppStrings.Plural("DaysCount", 12) == "12 дней", "ru: 12");
Check(AppStrings.Plural("DaysCount", 21) == "21 день", "ru: 21");
Check(AppStrings.Plural("StepsCount", 22) == "22 шага", "ru: 22");
Check(AppStrings.Plural("StepsCount", 0) == "0 шагов", "ru: 0");
LoadStrings("en");
Check(AppStrings.Plural("DaysCount", 1) == "1 day" && AppStrings.Plural("DaysCount", 2) == "2 days", "en plurals");
LoadStrings("de");
Check(AppStrings.Plural("StepsCount", 1) == "1 Schritt" && AppStrings.Plural("StepsCount", 3) == "3 Schritte", "de plurals");

// Completion dates must use the user's calendar day, not the UTC day.
var utcPlusTwo = TimeZoneInfo.CreateCustomTimeZone("UTC+02 test", TimeSpan.FromHours(2), "UTC+02 test", "UTC+02 test");
var nearMidnightUtc = new DateTimeOffset(2026, 10, 24, 22, 30, 0, TimeSpan.Zero);
Check(DateHelpers.ToIsoDate(nearMidnightUtc, utcPlusTwo) == "2026-10-25",
    "Completion date must cross midnight in the user's time zone.");

var programProgress = ProgressCalculator.CalculateProgramProgress(3);
Check(programProgress.CompletedDays == 3 && programProgress.Percent == 10 && programProgress.RemainingDays == 27,
    "Program progress must be completed program days out of 30.");
Check(ProgressCalculator.CalculateProgramProgress(30).Percent == 100, "A finished program is 100%.");
Check(ProgressCalculator.CountDistinctDates(["2026-10-15", "2026-10-18", "2026-10-18", "2026-10-18"]) == 2,
    "Calendar totals must count marked dates once.");
Check(ProgressCalculator.MondayBasedWeekdayIndex("2026-10-22") == 3
      && ProgressCalculator.MondayBasedWeekdayIndex("2026-10-25") == 6,
    "Week chart labels must follow each date, not the column index.");

// A completed day unlocks the next program day only after the local calendar day changes.
var pacingChallenge = new DailyChallenge
{
    ProgramDayNumber = 1,
    Status = ChallengeStatus.Completed,
    CompletedAt = DateTimeOffset.Now.ToString("O"),
};
Check(!ProgressCalculator.CanAdvanceToNextProgramDay(pacingChallenge),
    "A challenge completed today must not unlock another challenge today.");
pacingChallenge.CompletedAt = DateTimeOffset.Now.AddDays(-1).ToString("O");
Check(ProgressCalculator.CanAdvanceToNextProgramDay(pacingChallenge),
    "A completed challenge must unlock the next challenge on a later local day.");
pacingChallenge.ProgramDayNumber = DateHelpers.TargetMonthlyDays;
Check(ProgressCalculator.IsProgramCompleted(pacingChallenge)
      && !ProgressCalculator.CanAdvanceToNextProgramDay(pacingChallenge),
    "The final program day must be terminal.");

// Final assessment comparison must retain each question's individual change.
var finalSnapshot = SelfAssessmentSurvey.Create(SelfAssessmentKind.Final, [3, 2, 6, 4, 8]);
var assessmentChanges = SelfAssessmentSurvey.Compare(snapshot, finalSnapshot);
Check(assessmentChanges.Count == SelfAssessmentSurvey.Questions.Count,
    "Assessment comparison must include every question.");
Check(assessmentChanges.Select(change => change.Delta).SequenceEqual(new[] { 2, -1, 3, 1, 5 }),
    "Assessment comparison lost per-question score changes.");

Console.WriteLine("PASS: one 30-day program in ru/en/de, stable IDs, additive personalization, completion metadata, factory seeds and plurals.");
