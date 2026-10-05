using System.Text.Json;
using System.Text.Json.Serialization;
using ImproveYourself.Maui.Domain;

static void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
options.Converters.Add(new JsonStringEnumConverter());
var days = JsonSerializer.Deserialize<List<DailyChallenge>>(
    File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "course.json")), options)!;
Check(days.Count == 30, "The course must contain exactly 30 days.");
Check(days.Select(day => day.Id).Distinct().Count() == 30, "Day IDs must be unique.");
Check(days.SelectMany(day => day.Steps).Select(step => step.Id).Distinct().Count() == 90,
    "Step IDs must remain unique.");
var dayFiveQuote = days.OrderBy(day => day.Date).ElementAt(4).Steps.Single(step => step.Type == StepType.Quote);
Check(dayFiveQuote.QuoteAuthor == "Марк Аврелий",
    "Day 5 must attribute its quote to Marcus Aurelius.");
Check(string.IsNullOrWhiteSpace(dayFiveQuote.QuoteNote),
    "Day 5 must not describe the Marcus Aurelius quote as original program text.");

var snapshot = new SelfAssessmentSnapshot
{
    ConversationStartComfort = 1, OpinionExpressionEase = 3, BodyCalmUnderStress = 3,
    EyeContactEase = 3, SocialActionReadiness = 3,
};
ChallengeTemplateLanguage.SetLanguage("ru");
foreach (var (day, index) in days.OrderBy(day => day.Date).Select((day, index) => (day, index)))
{
    day.ProgramDayNumber = index + 1;
    Check(day.Steps.Count == 3 && day.Steps.Select(step => step.Type).Distinct().Count() == 3,
        $"Day {index + 1} must retain the three-step data contract.");
    Check(day.Steps.All(step => step.DailyChallengeId == day.Id), "Broken step parent.");
    var social = day.Steps.Single(step => step.Type == StepType.Social);
    Check(social.Description.Contains("Проще:") && social.Description.Contains("Сложнее (по желанию):"),
        $"Day {index + 1} lacks difficulty alternatives.");
    var before = JsonSerializer.Serialize(day);
    var personalized = ChallengePersonalizer.Personalize(day, snapshot);
    Check(JsonSerializer.Serialize(personalized) == before,
        $"Self-assessment overwrote structured day {index + 1}.");
    Check(!ReferenceEquals(day, personalized) && !ReferenceEquals(day.Steps[0], personalized.Steps[0]),
        "Personalization must preserve clone isolation.");
}
// Completion metadata must survive this path as well.
var completed = days[0];
completed.Status = ChallengeStatus.Completed;
completed.CompletedAt = "2026-10-03T12:00:00Z";
foreach (var step in completed.Steps)
{
    step.Status = StepStatus.Completed;
    step.CompletedAt = completed.CompletedAt;
}
Check(JsonSerializer.Serialize(ChallengePersonalizer.Personalize(completed, snapshot)) ==
      JsonSerializer.Serialize(completed), "Completion metadata changed.");

// The protection is intentionally limited to the Russian structured program.
foreach (var language in new[] { "en", "de" })
{
    ChallengeTemplateLanguage.SetLanguage(language);
    Check(!ChallengePersonalizer.UsesStructuredProgram(days[1]), "Other languages were opted out.");
    Check(ChallengePersonalizer.Personalize(days[1], snapshot).Steps[0].Description != days[1].Steps[0].Description,
        "Legacy personalization must still work for other languages.");
}
ChallengeTemplateLanguage.SetLanguage("ru");
var generated = ChallengeFactory.CreateDailyChallenge("2026-10-03");
Check(!ChallengePersonalizer.UsesStructuredProgram(generated), "Factory content is not a program day.");
Check(ChallengePersonalizer.Personalize(generated, snapshot).Steps[0].Description != generated.Steps[0].Description,
    "Factory personalization must still work.");
Console.WriteLine("PASS: 30-day contract, difficulty options, course preservation, completion metadata, clone isolation and legacy personalization.");
