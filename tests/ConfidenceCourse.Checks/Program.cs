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
        $"[{language}] Day and step IDs must match the Russian program.");
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

// Translations and personalized hints preserve the same assignment identity.
var russianPractice = courses["ru"][0].Steps.Single(step => step.Type == StepType.Practice);
var englishPractice = courses["en"][0].Steps.Single(step => step.Type == StepType.Practice);
Check(russianPractice.Id == englishPractice.Id, "Language copies must keep the practice step id.");
Check(StepTaskIdentity.SameAssignment(russianPractice, englishPractice),
    "A translated assignment must keep completion.");
Check(StepTaskIdentity.SameAssignment(russianPractice, russianPractice),
    "The same assignment text must keep its completion.");

var completedSocial = courses["ru"][0].Steps.Single(step => step.Type == StepType.Social);
completedSocial.Status = StepStatus.Completed;
var courseKey = StepTaskIdentity.VisibleKey(completedSocial);
completedSocial.CompletionTask = courseKey;
var hint = ChallengePersonalizer.CurrentHint(snapshot);
var hinted = ChallengePersonalizer.Personalize(courses["ru"][0], snapshot)
    .Steps.Single(step => step.Type == StepType.Social);
Check(hinted.Status == StepStatus.Completed, "Personalization must not strip a completion by itself.");
Check(StepTaskIdentity.CompletionBelongsTo(hinted, hint),
    "The completed wording keeps its mark when that text is shown again.");
var recordedHint = StepTaskIdentity.VisibleKey(hinted);
hinted.CompletionTask = recordedHint;
Check(StepTaskIdentity.CompletionBelongsTo(hinted, hint),
    "The same personalization hint must keep the completion.");
var otherSnapshot = new SelfAssessmentSnapshot
{
    ConversationStartComfort = 3, OpinionExpressionEase = 3, BodyCalmUnderStress = 3,
    EyeContactEase = 1, SocialActionReadiness = 3,
};
var otherHint = ChallengePersonalizer.CurrentHint(otherSnapshot);
var other = ChallengePersonalizer.Personalize(courses["ru"][0], otherSnapshot)
    .Steps.Single(step => step.Type == StepType.Social);
other.CompletionTask = recordedHint;
Check(!StepTaskIdentity.CompletionBelongsTo(other, otherHint),
    "A different personalization hint is not the completed task.");
other.CompletionTask = courseKey;
Check(StepTaskIdentity.CompletionBelongsTo(other, otherHint),
    "A wording recorded before hints were stored stays with that assignment.");
completedSocial.CompletionTask = null;

// Legacy text stamps migrate only when they match a known translation.
var translations = courses.Values.SelectMany(days => days).SelectMany(day => day.Steps).ToList();
foreach (var language in labels.Keys)
{
    var step = courses[language][0].Steps.Single(step => step.Type == StepType.Practice);
    step.Status = StepStatus.Completed;
    step.CompletionTask = StepTaskIdentity.VisibleKey(russianPractice);
    step.CompletionTask = StepTaskIdentity.UpgradeLegacyCompletion(step, translations);
    Check(StepTaskIdentity.CompletionBelongsTo(step), $"[{language}] Legacy Russian completion was lost.");
}
var hintCatalogs = labels.Keys.Select(language =>
    (IReadOnlyDictionary<string, string>)JsonSerializer.Deserialize<Dictionary<string, string>>(
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, $"strings.{language}.json")))!).ToList();
other.CompletionTask = recordedHint;
other.CompletionTask = StepTaskIdentity.UpgradeLegacyCompletion(other, translations, hintCatalogs);
LoadStrings("ru");
Check(other.CompletionTask == StepTaskIdentity.AssignmentKey(other, SelfAssessmentFocus.ConversationStart),
    "A legacy hint must migrate to its focus, not to a hint-free key.");
Check(!StepTaskIdentity.CompletionBelongsTo(other, otherHint), "A changed hint must clear a migrated assignment.");
Check(StepTaskIdentity.CompletionBelongsTo(other, hint), "The same focus must keep a migrated assignment.");
LoadStrings("en");
Check(StepTaskIdentity.CompletionBelongsTo(other, ChallengePersonalizer.CurrentHint(snapshot)),
    "The same focus must keep completion after a language change.");
other.CompletionTask = "unrecognized older task";
Check(StepTaskIdentity.UpgradeLegacyCompletion(other, translations) == other.CompletionTask
      && !StepTaskIdentity.CompletionBelongsTo(other), "An unrelated legacy task must not inherit completion.");
Check(!StepTaskIdentity.SameAssignment(russianPractice, courses["ru"][1].Steps.Single(step => step.Type == StepType.Practice)),
    "Different days must retain distinct assignment keys.");

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

var olderAssessment = SelfAssessmentSurvey.Create(SelfAssessmentKind.Start, [2, 2, 2, 2, 2]);
olderAssessment.CreatedAt = "2026-10-01T00:00:00.0000000Z";
var newerAssessment = SelfAssessmentSurvey.Create(SelfAssessmentKind.Start, [8, 8, 8, 8, 8]);
newerAssessment.CreatedAt = "2026-10-20T00:00:00.0000000Z";
Check(SelfAssessmentSurvey.ChooseLatest(null, newerAssessment) == newerAssessment,
    "A remote self-assessment must fill an empty local one.");
Check(SelfAssessmentSurvey.ChooseLatest(newerAssessment, olderAssessment) == newerAssessment,
    "An older remote self-assessment must not replace a newer local one.");
Check(SelfAssessmentSurvey.ChooseLatest(olderAssessment, newerAssessment) == newerAssessment,
    "A newer remote self-assessment must replace the local copy.");

var keptCompleted = CloudProgress.MergeStep(
    new StepProgressState(StepStatus.Completed, "2026-10-01T00:00:00Z", "2026-10-01T00:00:00Z"),
    new StepProgressState(StepStatus.NotStarted, null, "2026-11-01T00:00:00Z"),
    authoritative: false);
Check(keptCompleted.Status == StepStatus.Completed,
    "A fresh install must not downgrade a completed step while merging.");
var replaced = CloudProgress.MergeStep(
    keptCompleted,
    new StepProgressState(StepStatus.NotStarted, null, "2026-11-01T00:00:00Z"),
    authoritative: true);
Check(replaced.Status == StepStatus.NotStarted,
    "Switching accounts must replace the previous account's step progress.");
var undone = CloudProgress.MergeStep(
    new StepProgressState(StepStatus.NotStarted, null, "2026-11-02T00:00:00Z", LocalRegression: true),
    new StepProgressState(StepStatus.Completed, "2026-11-01T00:00:00Z", "2026-11-01T00:00:00Z"),
    authoritative: false);
Check(undone.Status == StepStatus.NotStarted,
    "An explicit undo must survive a sync that still has the step completed.");
var freshInstall = CloudProgress.MergeStep(
    new StepProgressState(StepStatus.NotStarted, null, "2026-11-02T00:00:00Z"),
    new StepProgressState(StepStatus.Completed, "2026-11-01T00:00:00Z", "2026-11-01T00:00:00Z"),
    authoritative: false);
Check(freshInstall.Status == StepStatus.Completed,
    "A new install must still restore a completed remote step.");

DailyChallenge Day(int number, ChallengeStatus status, string? completedAt) => new()
{
    ProgramDayNumber = number,
    Status = status,
    CompletedAt = completedAt,
};
var completedToday = new DateTimeOffset(2026, 11, 1, 12, 0, 0, TimeSpan.Zero);
var today = DateHelpers.ToLocalIsoDate(completedToday);
var yesterday = completedToday.AddDays(-1).ToString("o");
Check(CloudProgress.ResolveProgramDay([], today) == 1,
    "An empty program stays on day 1.");
Check(CloudProgress.ResolveProgramDay(
        [Day(1, ChallengeStatus.Completed, yesterday)],
        today) == 2,
    "A day completed before today restores the next program day.");
Check(CloudProgress.ResolveProgramDay(
        [Day(1, ChallengeStatus.Completed, completedToday.ToString("o"))],
        today) == 1,
    "A day completed today stays current until the next local day.");
Check(CloudProgress.ResolveProgramDay(
        [
            Day(1, ChallengeStatus.Completed, yesterday),
            Day(2, ChallengeStatus.InProgress, null),
        ],
        today) == 2,
    "An in-progress day stays the current program day.");
Check(CloudProgress.ResolveProgramDay(
        [Day(30, ChallengeStatus.Completed, yesterday)],
        today) == 30,
    "A finished program stays on day 30.");

LoadStrings("en");
Check(AccountInput.ValidateRegistration("", "12345678") == AppStrings.AuthEmailRequired, "Empty registration email must be rejected.");
Check(AccountInput.ValidateRegistration("abc", "1") == AppStrings.AuthEmailInvalid, "A malformed email must be rejected before the server.");
Check(AccountInput.ValidateRegistration("имя@example.com", "12345678") == AppStrings.AuthEmailInvalid, "A non-ASCII email must be rejected on the client.");
Check(AccountInput.ValidateRegistration("person@example.com", "1") == AppStrings.AuthPasswordTooShort, "A short password must be rejected on the client.");
Check(AccountInput.ValidateRegistration("person@example.com", "12345678") is null, "A valid email and password must pass client validation.");
Check(AccountInput.ValidateDisplayName("   ") == AppStrings.NameRequired, "A blank name must not be saved.");
Check(AccountInput.FirstGrapheme("🙂Ada") == "🙂", "The avatar must use the first grapheme, including emoji.");
Check(ProgressCalculator.GetNextStepStatus(StepStatus.Completed) == StepStatus.NotStarted, "A completed step must be undoable.");
Check(ProgressCalculator.GetNextStepStatus(StepStatus.NotStarted) == StepStatus.Completed, "An open step must still be completable.");
var orderedSteps = new List<ChallengeStep>
{
    new() { Type = StepType.Practice, SortOrder = 1, Status = StepStatus.NotStarted },
    new() { Type = StepType.Social, SortOrder = 2, Status = StepStatus.NotStarted },
};
Check(ProgressCalculator.IsStepBlocked(orderedSteps, StepType.Social), "The second step stays locked until the first is done.");
orderedSteps[0].Status = StepStatus.Completed;
Check(!ProgressCalculator.IsStepBlocked(orderedSteps, StepType.Social), "The second step opens after the first is done.");
foreach (var (language, phrase) in new[] { ("en", "Two simple steps"), ("ru", "Два простых шага"), ("de", "Zwei einfache Schritte") })
{
    LoadStrings(language);
    Check(AppStrings.Onboarding2_Body.Contains(phrase), $"[{language}] Onboarding must describe two steps.");
    Check(!AppStrings.Onboarding2_Body.Contains("Three") && !AppStrings.Onboarding2_Body.Contains("Три ") && !AppStrings.Onboarding2_Body.Contains("Drei "),
        $"[{language}] Onboarding must not promise three steps.");
}

// SQLite regressions: localized legacy stamps, process-style reopen, undo, and weekly stats.
SQLitePCL.Batteries_V2.Init();
var testLocalization = new ConfidenceCourse.Checks.TestLocalization();
LoadStrings("en");
var repository = new ImproveYourself.Maui.Persistence.SqliteChallengeRepository(testLocalization);
repository.Initialize();
var storedDay = repository.GetChallengeForProgramDay(1);
var storedPractice = storedDay.Steps.Single(step => step.Type == StepType.Practice);
repository.AdvanceChallengeStepStatus(storedDay.Date, StepType.Practice, StepStatus.Completed);
repository.RememberStepTask(storedPractice.Id, StepTaskIdentity.VisibleKey(storedPractice));
var originalCompletedAt = repository.GetChallengeForProgramDay(1).Steps.Single(step => step.Type == StepType.Practice).CompletedAt;
foreach (var language in new[] { "ru", "de", "en" })
{
    LoadStrings(language);
    testLocalization.SetLanguage(language);
    repository.ReloadBundledContent();
    repository.RelocalizeChallenges();
    repository = new ImproveYourself.Maui.Persistence.SqliteChallengeRepository(testLocalization);
    var reloaded = repository.GetChallengeForProgramDay(1, snapshot);
    Check(ProgressCalculator.CountCompletedSteps(reloaded.Steps) == 1,
        $"[{language}] SQLite lost partial progress after a language change and reopen.");
    var migratedPractice = reloaded.Steps.Single(step => step.Type == StepType.Practice);
    Check(migratedPractice.CompletionTask == StepTaskIdentity.AssignmentKey(migratedPractice)
        && migratedPractice.CompletedAt == originalCompletedAt,
        $"[{language}] Migration must persist a stable key and preserve the completion timestamp.");
    Check(!ProgressCalculator.IsStepBlocked(reloaded.Steps, StepType.Social),
        $"[{language}] Language change relocked the second step.");
    repository.AdvanceChallengeStepStatus(storedDay.Date, StepType.Social, StepStatus.Completed);
    repository = new ImproveYourself.Maui.Persistence.SqliteChallengeRepository(testLocalization);
    Check(repository.GetChallengeForProgramDay(1).Status == ChallengeStatus.Completed,
        $"[{language}] Completed day was lost on reopen.");
    repository.AdvanceChallengeStepStatus(storedDay.Date, StepType.Social, StepStatus.NotStarted);
}
var statsToday = DateHelpers.LocalTodayIsoDate;
var weekly = ProgressCalculator.BuildWeeklyStats(repository.ListChallengesBetween(DateHelpers.AddDays(statsToday, -6), statsToday), statsToday);
Check(weekly.CategoryBreakdown[StepType.Practice] == 1 && weekly.CategoryBreakdown[StepType.Social] == 0,
    "Undoing step 2 must preserve step 1 in weekly categories.");
Check(weekly.TotalCompletedDays == 0 && weekly.Days[^1].CompletedSteps == 1 && weekly.Days[^1].TotalSteps == 2,
    "A partial day must show one of two steps, without counting as a completed day.");
var splitDay = new DailyChallenge
{
    Status = ChallengeStatus.Completed, CompletedAt = DateTimeOffset.Now.ToString("o"), Date = "2000-01-01",
    Steps = [
        new() { Type = StepType.Practice, Status = StepStatus.Completed, CompletedAt = DateTimeOffset.Now.AddDays(-8).ToString("o") },
        new() { Type = StepType.Social, Status = StepStatus.Completed, CompletedAt = DateTimeOffset.Now.ToString("o") }
    ]
};
var splitStats = ProgressCalculator.BuildWeeklyStats([splitDay], statsToday);
Check(splitStats.TotalCompletedDays == 1 && splitStats.CategoryBreakdown[StepType.Practice] == 0
    && splitStats.CategoryBreakdown[StepType.Social] == 1, "Categories must use each step's date, not the completed day's date.");
Check(splitStats.Days[^1].IsCompleted && splitStats.Days[^1].CompletedSteps == 2 && splitStats.Days[^1].TotalSteps == 2,
    "A finished day fills its column on the completion date.");
var todayDate = DateHelpers.ParseIsoDate(statsToday);
var localOffset = DateTimeOffset.Now.Offset;
var spreadDay = new DailyChallenge
{
    Status = ChallengeStatus.Completed,
    CompletedAt = new DateTimeOffset(todayDate.ToDateTime(new TimeOnly(15, 0)), localOffset).ToString("o"),
    Date = "2000-01-02",
    Steps = [
        new()
        {
            Type = StepType.Practice,
            Status = StepStatus.Completed,
            CompletedAt = new DateTimeOffset(todayDate.AddDays(-1).ToDateTime(new TimeOnly(15, 0)), localOffset).ToString("o"),
        },
        new()
        {
            Type = StepType.Social,
            Status = StepStatus.Completed,
            CompletedAt = new DateTimeOffset(todayDate.ToDateTime(new TimeOnly(18, 0)), localOffset).ToString("o"),
        }
    ]
};
var spreadStats = ProgressCalculator.BuildWeeklyStats([spreadDay], statsToday);
Check(spreadStats.Days[^1].IsCompleted && spreadStats.Days[^1].CompletedSteps == 2 && spreadStats.Days[^1].TotalSteps == 2
    && spreadStats.Days[^2].CompletedSteps == 0 && !spreadStats.Days[^2].IsCompleted,
    "Steps of a finished day stay on the day the day was completed.");
LoadStrings("en");
repository.AdvanceChallengeStepStatus(storedDay.Date, StepType.Social, StepStatus.Completed);
var completedSocialStep = repository.GetChallengeForProgramDay(1, snapshot).Steps.Single(step => step.Type == StepType.Social);
repository.RememberStepTask(completedSocialStep.Id, StepTaskIdentity.VisibleKey(completedSocialStep));
foreach (var language in new[] { "ru", "de", "en" })
{
    LoadStrings(language);
    testLocalization.SetLanguage(language);
    repository.ReloadBundledContent();
    repository.RelocalizeChallenges();
    var kept = repository.GetChallengeForProgramDay(1, snapshot);
    var keptSocial = kept.Steps.Single(step => step.Type == StepType.Social);
    Check(kept.Status == ChallengeStatus.Completed
        && keptSocial.Status == StepStatus.Completed
        && keptSocial.CompletionTask == StepTaskIdentity.AssignmentKey(keptSocial, SelfAssessmentFocus.ConversationStart),
        $"[{language}] The same focus must keep the social step across languages.");
    var changed = repository.GetChallengeForProgramDay(1, otherSnapshot);
    Check(changed.Steps.Single(step => step.Type == StepType.Social).Status != StepStatus.Completed
        && changed.Status != ChallengeStatus.Completed,
        $"[{language}] A different focus must clear the social step.");
}
foreach (var language in labels.Keys)
{
    LoadStrings(language);
    Check(AccountInput.ValidateLogin("tester@example.com", "") == AppStrings.AuthPasswordRequired,
        $"[{language}] An empty password must not produce an email error.");
    Check(AccountInput.ValidateLogin("", "") == AppStrings.AuthEmailRequired, "Empty email must be reported first.");
    Check(AccountInput.ValidateLogin("bad", "password") == AppStrings.AuthEmailInvalid, "Invalid login email must be rejected.");
    Check(AccountInput.ValidateLogin(" tester@example.com ", "x") is null, "Login must accept trimmed emails and existing short passwords.");
}

Console.WriteLine("PASS: SQLite language migration/reopen, partial-day statistics, login validation; one 30-day program in ru/en/de, stable IDs, additive personalization, completion metadata, factory seeds and plurals.");
