using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using ImproveYourself.Maui.Application;
using ImproveYourself.Maui.Domain;
using ImproveYourself.Maui.Resources.Strings;
using Microsoft.Maui.Storage;
using SQLite;

namespace ImproveYourself.Maui.Persistence;

[Table("daily_challenges")]
internal sealed class DailyChallengeRecord
{
    [PrimaryKey]
    [Column("id")]
    public string Id { get; set; } = string.Empty;

    [Unique]
    [Column("date")]
    public string Date { get; set; } = string.Empty;

    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("status")]
    public string Status { get; set; } = string.Empty;

    [Column("created_at")]
    public string CreatedAt { get; set; } = string.Empty;

    [Column("updated_at")]
    public string UpdatedAt { get; set; } = string.Empty;
}

[Table("challenge_steps")]
internal sealed class ChallengeStepRecord
{
    [PrimaryKey]
    [Column("id")]
    public string Id { get; set; } = string.Empty;

    [Indexed]
    [Column("daily_challenge_id")]
    public string DailyChallengeId { get; set; } = string.Empty;

    [Column("type")]
    public string Type { get; set; } = string.Empty;

    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("subtitle")]
    public string? Subtitle { get; set; }

    [Column("description")]
    public string Description { get; set; } = string.Empty;

    [Column("tip")]
    public string? Tip { get; set; }

    [Column("duration_seconds")]
    public int? DurationSeconds { get; set; }

    [Column("quote_text")]
    public string? QuoteText { get; set; }

    [Column("quote_author")]
    public string? QuoteAuthor { get; set; }

    [Column("quote_note")]
    public string? QuoteNote { get; set; }

    [Column("sort_order")]
    public int SortOrder { get; set; }

    [Column("status")]
    public string Status { get; set; } = string.Empty;

    [Column("completed_at")]
    public string? CompletedAt { get; set; }

    [Column("updated_at")]
    public string UpdatedAt { get; set; } = string.Empty;
}

public sealed class SqliteChallengeRepository : IChallengeRepository
{
    private const int QuoteStorageSortOrder = 0;

    private static readonly JsonSerializerOptions BundledChallengeJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private const string FallbackBundledLanguage = "en";

    private readonly SQLiteConnection _database;
    private readonly ILocalizationService _localizationService;
    private IReadOnlyDictionary<string, DailyChallenge> _bundledChallenges =
        new Dictionary<string, DailyChallenge>(StringComparer.Ordinal);
    private bool _initialized;

    public SqliteChallengeRepository(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
        var databasePath = Path.Combine(FileSystem.AppDataDirectory, "improveyourself.db");
        _database = new SQLiteConnection(databasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex);
    }

    public void ReloadBundledContent()
    {
        _bundledChallenges = LoadBundledChallengesForLanguage(_localizationService.CurrentLanguage);

        if (_initialized)
        {
            SeedBundledChallenges();
            RelocalizePristineFactoryChallenges();
        }
    }

    public void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        _database.Execute("PRAGMA foreign_keys = ON;");
        _database.CreateTable<DailyChallengeRecord>();
        _database.CreateTable<ChallengeStepRecord>();
        EnsureSchema();

        _database.Execute("CREATE INDEX IF NOT EXISTS idx_daily_challenges_date ON daily_challenges (date);");
        _database.Execute("CREATE INDEX IF NOT EXISTS idx_challenge_steps_challenge ON challenge_steps (daily_challenge_id, sort_order);");

        _bundledChallenges = LoadBundledChallengesForLanguage(_localizationService.CurrentLanguage);
        SeedBundledChallenges();
        RelocalizePristineFactoryChallenges();

        _initialized = true;
    }

    public DailyChallenge? GetChallengeByDate(string date)
    {
        Initialize();

        var row = _database.Table<DailyChallengeRecord>().FirstOrDefault(record => record.Date == date);

        return row is null ? null : MapChallengeRow(row);
    }

    public DailyChallenge GetOrCreateChallenge(string date, SelfAssessmentSnapshot? personalizationSnapshot = null)
    {
        Initialize();

        var existing = GetChallengeByDate(date);

        if (existing is not null)
        {
            return PersonalizeExistingChallengeIfNeeded(existing, personalizationSnapshot);
        }

        var challenge = GetBundledChallenge(date) ?? ChallengeFactory.CreateDailyChallenge(date);
        challenge = ChallengePersonalizer.Personalize(challenge, personalizationSnapshot);
        InsertChallenge(challenge);

        return GetChallengeByDate(date) ?? challenge;
    }

    public void ApplyPersonalization(SelfAssessmentSnapshot snapshot)
    {
        Initialize();

        var rows = _database.Query<DailyChallengeRecord>(
            "SELECT id, date, title, status, created_at, updated_at FROM daily_challenges ORDER BY date ASC");

        foreach (var row in rows)
        {
            var challenge = MapChallengeRow(row);
            PersonalizeExistingChallengeIfNeeded(challenge, snapshot);
        }
    }

    public DailyChallenge AdvanceChallengeStepStatus(string date, StepType stepType)
    {
        var challenge = GetOrCreateChallenge(date);
        var targetStep = challenge.Steps.FirstOrDefault(step => step.Type == stepType);

        if (targetStep is null)
        {
            return challenge;
        }

        var nextStatus = ProgressCalculator.GetNextStepStatus(targetStep.Status);
        var updatedAt = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
        var completedAt = nextStatus == StepStatus.Completed
            ? updatedAt
            : null;

        _database.Execute(
            "UPDATE challenge_steps SET status = ?, completed_at = ?, updated_at = ? WHERE id = ?",
            nextStatus.ToStorage(),
            completedAt,
            updatedAt,
            targetStep.Id);
        _database.Execute(
            "UPDATE daily_challenges SET updated_at = ? WHERE date = ?",
            updatedAt,
            date);

        return GetOrCreateChallenge(date);
    }

    public IReadOnlyList<string> ListCompletedDates()
    {
        Initialize();

        var rows = _database.Query<DailyChallengeRecord>(
            "SELECT id, date, title, status, created_at, updated_at FROM daily_challenges ORDER BY date ASC");

        return rows
            .Select(MapChallengeRow)
            .Where(challenge => challenge.Status == ChallengeStatus.Completed)
            .Select(challenge => challenge.Date)
            .ToList();
    }

    public IReadOnlyList<DailyChallenge> ListChallengesBetween(string startDate, string endDate)
    {
        Initialize();

        var rows = _database.Query<DailyChallengeRecord>(
            "SELECT id, date, title, status, created_at, updated_at FROM daily_challenges WHERE date BETWEEN ? AND ? ORDER BY date ASC",
            startDate,
            endDate);

        return rows.Select(MapChallengeRow).ToList();
    }

    public IReadOnlyList<DailyChallenge> ListAllChallenges()
    {
        Initialize();

        var rows = _database.Query<DailyChallengeRecord>(
            "SELECT id, date, title, status, created_at, updated_at FROM daily_challenges ORDER BY date ASC");

        return rows.Select(MapChallengeRow).ToList();
    }

    public int GetCompletedChallengesCount()
    {
        Initialize();

        return ListCompletedDates().Count;
    }

    private DailyChallenge? GetBundledChallenge(string date)
    {
        if (!_bundledChallenges.TryGetValue(date, out var challenge))
        {
            return null;
        }

        return CloneChallenge(challenge);
    }

    private void SeedBundledChallenges()
    {
        foreach (var challenge in _bundledChallenges.Values)
        {
            var existing = TryGetStoredChallengeByDate(challenge.Date);

            if (existing is null)
            {
                InsertChallenge(CloneChallenge(challenge));
                continue;
            }

            if (IsPristineChallenge(existing))
            {
                ReplaceChallenge(existing, CloneChallenge(challenge));
                continue;
            }

            SyncQuoteMetadata(existing, challenge);
        }
    }

    private void RelocalizePristineFactoryChallenges()
    {
        var rows = _database.Query<DailyChallengeRecord>(
            "SELECT id, date, title, status, created_at, updated_at FROM daily_challenges ORDER BY date ASC");

        foreach (var row in rows)
        {
            if (_bundledChallenges.ContainsKey(row.Date))
            {
                continue;
            }

            var existing = MapChallengeRow(row);

            if (!IsPristineChallenge(existing))
            {
                continue;
            }

            ReplaceChallenge(existing, ChallengeFactory.CreateDailyChallenge(existing.Date));
        }
    }

    private void EnsureSchema()
    {
        if (!ColumnExists("challenge_steps", "quote_note"))
        {
            _database.Execute("ALTER TABLE challenge_steps ADD COLUMN quote_note TEXT;");
        }

        if (!ColumnExists("daily_challenges", "updated_at"))
        {
            _database.Execute("ALTER TABLE daily_challenges ADD COLUMN updated_at TEXT NOT NULL DEFAULT '';");
        }

        if (!ColumnExists("challenge_steps", "updated_at"))
        {
            _database.Execute("ALTER TABLE challenge_steps ADD COLUMN updated_at TEXT NOT NULL DEFAULT '';");
        }

        var now = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
        _database.Execute(
            "UPDATE daily_challenges SET updated_at = CASE WHEN created_at IS NULL OR created_at = '' THEN ? ELSE created_at END WHERE updated_at IS NULL OR updated_at = ''",
            now);
        _database.Execute(
            "UPDATE challenge_steps SET updated_at = CASE WHEN completed_at IS NULL OR completed_at = '' THEN ? ELSE completed_at END WHERE updated_at IS NULL OR updated_at = ''",
            now);
    }

    private bool ColumnExists(string tableName, string columnName) =>
        _database.ExecuteScalar<int>($"SELECT COUNT(*) FROM pragma_table_info('{tableName}') WHERE name = ?", columnName) > 0;

    private DailyChallenge? TryGetStoredChallengeByDate(string date)
    {
        var row = _database.Table<DailyChallengeRecord>().FirstOrDefault(record => record.Date == date);

        return row is null ? null : MapChallengeRow(row);
    }

    private static bool IsPristineChallenge(DailyChallenge challenge) =>
        challenge.Status == ChallengeStatus.NotStarted
        && challenge.Steps.All(step => step.Status == StepStatus.NotStarted);

    private void SyncQuoteMetadata(DailyChallenge existingChallenge, DailyChallenge bundledChallenge)
    {
        var bundledQuoteStep = BuildQuoteStorageStep(bundledChallenge);

        if (bundledQuoteStep is null)
        {
            return;
        }

        var updated = _database.Execute(
            "UPDATE challenge_steps SET title = ?, description = ?, quote_text = ?, quote_author = ?, quote_note = ?, sort_order = ?, updated_at = ? WHERE daily_challenge_id = ? AND type = ?",
            bundledQuoteStep.Title,
            bundledQuoteStep.Description,
            bundledQuoteStep.QuoteText,
            bundledQuoteStep.QuoteAuthor,
            bundledQuoteStep.QuoteNote,
            bundledQuoteStep.SortOrder,
            bundledQuoteStep.UpdatedAt,
            existingChallenge.Id,
            StepType.Quote.ToStorage());

        if (updated > 0)
        {
            return;
        }

        _database.Execute(
            "INSERT OR IGNORE INTO challenge_steps (id, daily_challenge_id, type, title, subtitle, description, tip, duration_seconds, quote_text, quote_author, quote_note, sort_order, status, completed_at, updated_at) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
            bundledQuoteStep.Id,
            bundledQuoteStep.DailyChallengeId,
            bundledQuoteStep.Type.ToStorage(),
            bundledQuoteStep.Title,
            bundledQuoteStep.Subtitle,
            bundledQuoteStep.Description,
            bundledQuoteStep.Tip,
            bundledQuoteStep.DurationSeconds,
            bundledQuoteStep.QuoteText,
            bundledQuoteStep.QuoteAuthor,
            bundledQuoteStep.QuoteNote,
            bundledQuoteStep.SortOrder,
            bundledQuoteStep.Status.ToStorage(),
            bundledQuoteStep.CompletedAt,
            bundledQuoteStep.UpdatedAt);
    }

    private void ReplaceChallenge(DailyChallenge existingChallenge, DailyChallenge bundledChallenge)
    {
        _database.Execute("DELETE FROM challenge_steps WHERE daily_challenge_id = ?", existingChallenge.Id);
        _database.Execute("DELETE FROM daily_challenges WHERE id = ?", existingChallenge.Id);
        InsertChallenge(bundledChallenge);
    }

    private DailyChallenge PersonalizeExistingChallengeIfNeeded(
        DailyChallenge challenge,
        SelfAssessmentSnapshot? personalizationSnapshot)
    {
        if (personalizationSnapshot is null || !IsPristineChallenge(challenge))
        {
            return challenge;
        }

        var personalized = ChallengePersonalizer.Personalize(challenge, personalizationSnapshot);

        if (!HasChallengeContentChanges(challenge, personalized))
        {
            return challenge;
        }

        TouchChallenge(personalized);
        ReplaceChallenge(challenge, personalized);

        return GetChallengeByDate(challenge.Date) ?? personalized;
    }

    private static void TouchChallenge(DailyChallenge challenge)
    {
        var updatedAt = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
        challenge.UpdatedAt = updatedAt;

        foreach (var step in challenge.Steps)
        {
            step.UpdatedAt = updatedAt;
        }
    }

    private static bool HasChallengeContentChanges(DailyChallenge current, DailyChallenge next)
    {
        if (current.Title != next.Title
            || current.QuoteText != next.QuoteText
            || current.QuoteAuthor != next.QuoteAuthor
            || current.QuoteNote != next.QuoteNote
            || current.Steps.Count != next.Steps.Count)
        {
            return true;
        }

        var nextStepsById = next.Steps.ToDictionary(step => step.Id, StringComparer.Ordinal);

        foreach (var currentStep in current.Steps)
        {
            if (!nextStepsById.TryGetValue(currentStep.Id, out var nextStep)
                || currentStep.Title != nextStep.Title
                || currentStep.Subtitle != nextStep.Subtitle
                || currentStep.Description != nextStep.Description
                || currentStep.Tip != nextStep.Tip
                || currentStep.DurationSeconds != nextStep.DurationSeconds
                || currentStep.SortOrder != nextStep.SortOrder)
            {
                return true;
            }
        }

        return false;
    }

    private static IReadOnlyDictionary<string, DailyChallenge> LoadBundledChallengesForLanguage(string language)
    {
        var challenges = LoadBundledChallenges($"daily-challenges.{language}.json");

        if (challenges.Count == 0 && !string.Equals(language, FallbackBundledLanguage, StringComparison.Ordinal))
        {
            challenges = LoadBundledChallenges($"daily-challenges.{FallbackBundledLanguage}.json");
        }

        return challenges;
    }

    private static IReadOnlyDictionary<string, DailyChallenge> LoadBundledChallenges(string fileName)
    {
        try
        {
            using var stream = FileSystem.OpenAppPackageFileAsync(fileName).GetAwaiter().GetResult();
            var parsedChallenges = JsonSerializer.Deserialize<List<DailyChallenge>>(stream, BundledChallengeJsonOptions) ?? [];
            var bundledChallenges = new Dictionary<string, DailyChallenge>(StringComparer.Ordinal);

            foreach (var challenge in parsedChallenges)
            {
                if (string.IsNullOrWhiteSpace(challenge.Date))
                {
                    continue;
                }

                var normalized = NormalizeChallenge(challenge);
                bundledChallenges[normalized.Date] = normalized;
            }

            return bundledChallenges;
        }
        catch
        {
            return new Dictionary<string, DailyChallenge>(StringComparer.Ordinal);
        }
    }

    private static DailyChallenge NormalizeChallenge(DailyChallenge challenge)
    {
        var challengeId = string.IsNullOrWhiteSpace(challenge.Id)
            ? $"challenge-{challenge.Date}"
            : challenge.Id;
        var createdAt = string.IsNullOrWhiteSpace(challenge.CreatedAt)
            ? DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)
            : challenge.CreatedAt;
        var updatedAt = string.IsNullOrWhiteSpace(challenge.UpdatedAt)
            ? createdAt
            : challenge.UpdatedAt;

        var normalizedSteps = challenge.Steps
            .OrderBy(step => step.SortOrder)
            .Select((step, index) => NormalizeStep(step, challengeId, index + 1))
            .ToList();

        var quoteStep = normalizedSteps.FirstOrDefault(step => step.Type == StepType.Quote);
        var visibleSteps = normalizedSteps
            .Where(step => step.Type != StepType.Quote)
            .Select((step, index) =>
            {
                step.SortOrder = index + 1;
                return step;
            })
            .ToList();

        var quoteText = string.IsNullOrWhiteSpace(challenge.QuoteText)
            ? quoteStep?.QuoteText
            : challenge.QuoteText;
        var quoteAuthor = string.IsNullOrWhiteSpace(challenge.QuoteAuthor)
            ? quoteStep?.QuoteAuthor
            : challenge.QuoteAuthor;
        var quoteNote = string.IsNullOrWhiteSpace(challenge.QuoteNote)
            ? quoteStep?.QuoteNote
            : challenge.QuoteNote;

        return new DailyChallenge
        {
            Id = challengeId,
            Date = challenge.Date,
            Title = ChallengeTextLocalizer.GetDisplayTitle(
                string.IsNullOrWhiteSpace(challenge.Title) ? null : challenge.Title),
            Status = ProgressCalculator.GetChallengeStatus(visibleSteps),
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            QuoteText = string.IsNullOrWhiteSpace(quoteText) ? null : quoteText,
            QuoteAuthor = NormalizeQuoteAuthor(quoteAuthor),
            QuoteNote = string.IsNullOrWhiteSpace(quoteNote) ? null : quoteNote,
            Steps = visibleSteps,
        };
    }

    private static ChallengeStep NormalizeStep(ChallengeStep step, string challengeId, int fallbackSortOrder) => new()
    {
        Id = string.IsNullOrWhiteSpace(step.Id)
            ? $"{challengeId}-{step.Type.ToString().ToLowerInvariant()}"
            : step.Id,
        DailyChallengeId = string.IsNullOrWhiteSpace(step.DailyChallengeId) ? challengeId : step.DailyChallengeId,
        Type = step.Type,
        Title = NormalizeStepTitle(step.Type, step.Title),
        Subtitle = step.Subtitle,
        Description = step.Description,
        Tip = step.Tip,
        DurationSeconds = step.DurationSeconds,
        QuoteText = step.QuoteText,
        QuoteAuthor = NormalizeQuoteAuthor(step.QuoteAuthor),
        QuoteNote = step.QuoteNote,
        SortOrder = step.SortOrder <= 0 ? fallbackSortOrder : step.SortOrder,
        Status = step.Status,
        CompletedAt = step.CompletedAt,
        UpdatedAt = string.IsNullOrWhiteSpace(step.UpdatedAt)
            ? DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)
            : step.UpdatedAt,
    };

    private static string NormalizeStepTitle(StepType stepType, string? title) => stepType switch
    {
        StepType.Practice => string.IsNullOrWhiteSpace(title) ? AppStrings.PracticeStep_Title : title,
        StepType.Social => AppStrings.SocialStep_Title,
        StepType.Quote => AppStrings.QuoteStep_Title,
        _ => string.IsNullOrWhiteSpace(title) ? AppStrings.StepOfDay : title,
    };

    private static string? NormalizeQuoteAuthor(string? author)
    {
        if (string.IsNullOrWhiteSpace(author))
        {
            return null;
        }

        var trimmed = author.Trim();

        return trimmed switch
        {
            "Современная интерпретация" => null,
            "В духе Брюса Ли" => "Брюс Ли",
            "В духе Брене Браун" => "Брене Браун",
            "В духе Конфуция" => "Конфуций",
            "В духе Майи Энджелоу" => "Майя Энджелоу",
            "В духе Марка Аврелия" => "Марк Аврелий",
            "В духе Марка Твена" => "Марк Твен",
            "В духе Нельсона Манделы" => "Нельсон Мандела",
            "В духе Роберта Фроста" => "Роберт Фрост",
            "В духе Сенеки" => "Сенека",
            "В духе Теодора Рузвельта" => "Теодор Рузвельт",
            "В духе Тима Ферриса" => "Тим Феррис",
            "В духе Уильяма Джеймса" => "Уильям Джеймс",
            "В духе Уэйна Гретцки" => "Уэйн Гретцки",
            "В духе Франклина Рузвельта" => "Франклин Рузвельт",
            "В духе Фридриха Ницше" => "Фридрих Ницше",
            "В духе Элеоноры Рузвельт" => "Элеонора Рузвельт",
            "В духе Эпиктета" => "Эпиктет",
            _ => trimmed.StartsWith("В духе ", StringComparison.OrdinalIgnoreCase) ? null : trimmed,
        };
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

    private static ChallengeStep MapStepRow(ChallengeStepRecord row) => new()
    {
        Id = row.Id,
        DailyChallengeId = row.DailyChallengeId,
        Type = row.Type.ToStepType(),
        Title = row.Title,
        Subtitle = row.Subtitle,
        Description = row.Description,
        Tip = row.Tip,
        DurationSeconds = row.DurationSeconds,
        QuoteText = row.QuoteText,
        QuoteAuthor = row.QuoteAuthor,
        QuoteNote = row.QuoteNote,
        SortOrder = row.SortOrder,
        Status = row.Status.ToStepStatus(),
        CompletedAt = row.CompletedAt,
        UpdatedAt = row.UpdatedAt,
    };

    private DailyChallenge MapChallengeRow(DailyChallengeRecord row)
    {
        var challenge = NormalizeChallenge(new DailyChallenge
        {
            Id = row.Id,
            Date = row.Date,
            Title = ChallengeTextLocalizer.GetDisplayTitle(row.Title),
            Status = row.Status.ToChallengeStatus(),
            CreatedAt = row.CreatedAt,
            UpdatedAt = row.UpdatedAt,
            Steps = LoadChallengeSteps(row.Id),
        });

        return SynchronizeChallengeStatus(challenge);
    }

    private List<ChallengeStep> LoadChallengeSteps(string challengeId)
    {
        var rows = _database.Table<ChallengeStepRecord>()
            .Where(record => record.DailyChallengeId == challengeId)
            .OrderBy(record => record.SortOrder)
            .ToList();

        return rows.Select(MapStepRow).ToList();
    }

    private DailyChallenge SynchronizeChallengeStatus(DailyChallenge challenge)
    {
        var status = ProgressCalculator.GetChallengeStatus(challenge.Steps);

        if (status != challenge.Status)
        {
            var updatedAt = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
            _database.Execute(
                "UPDATE daily_challenges SET status = ?, updated_at = ? WHERE id = ?",
                status.ToStorage(),
                updatedAt,
                challenge.Id);

            challenge.Status = status;
            challenge.UpdatedAt = updatedAt;
        }

        return challenge;
    }

    private static IEnumerable<ChallengeStep> BuildStorageSteps(DailyChallenge challenge)
    {
        foreach (var step in challenge.Steps
                     .Where(step => step.Type != StepType.Quote)
                     .Select(CloneStep))
        {
            yield return step;
        }

        var quoteStep = BuildQuoteStorageStep(challenge);

        if (quoteStep is not null)
        {
            yield return quoteStep;
        }
    }

    private static ChallengeStep? BuildQuoteStorageStep(DailyChallenge challenge)
    {
        if (string.IsNullOrWhiteSpace(challenge.QuoteText))
        {
            return null;
        }

        return new ChallengeStep
        {
            Id = $"{challenge.Id}-quote",
            DailyChallengeId = challenge.Id,
            Type = StepType.Quote,
            Title = AppStrings.QuoteStep_Title,
            Description = AppStrings.QuoteStep_Description,
            QuoteText = challenge.QuoteText,
            QuoteAuthor = challenge.QuoteAuthor,
            QuoteNote = challenge.QuoteNote,
            SortOrder = QuoteStorageSortOrder,
            Status = StepStatus.NotStarted,
            CompletedAt = null,
            UpdatedAt = string.IsNullOrWhiteSpace(challenge.UpdatedAt)
                ? DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)
                : challenge.UpdatedAt,
        };
    }

    private void InsertChallenge(DailyChallenge challenge)
    {
        _database.Execute(
            "INSERT OR IGNORE INTO daily_challenges (id, date, title, status, created_at, updated_at) VALUES (?, ?, ?, ?, ?, ?)",
            challenge.Id,
            challenge.Date,
            challenge.Title,
            challenge.Status.ToStorage(),
            challenge.CreatedAt,
            challenge.UpdatedAt);

        foreach (var step in BuildStorageSteps(challenge))
        {
            _database.Execute(
                "INSERT OR IGNORE INTO challenge_steps (id, daily_challenge_id, type, title, subtitle, description, tip, duration_seconds, quote_text, quote_author, quote_note, sort_order, status, completed_at, updated_at) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                step.Id,
                step.DailyChallengeId,
                step.Type.ToStorage(),
                step.Title,
                step.Subtitle,
                step.Description,
                step.Tip,
                step.DurationSeconds,
                step.QuoteText,
                step.QuoteAuthor,
                step.QuoteNote,
                step.SortOrder,
                step.Status.ToStorage(),
                step.CompletedAt,
                step.UpdatedAt);
        }
    }
}
