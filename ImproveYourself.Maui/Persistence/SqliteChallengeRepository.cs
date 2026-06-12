using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using ImproveYourself.Maui.Domain;
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

    [Column("sort_order")]
    public int SortOrder { get; set; }

    [Column("status")]
    public string Status { get; set; } = string.Empty;

    [Column("completed_at")]
    public string? CompletedAt { get; set; }
}

public sealed class SqliteChallengeRepository : IChallengeRepository
{
    private const string BundledChallengeFileName = "daily-challenges.ru.json";

    private static readonly JsonSerializerOptions BundledChallengeJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly SQLiteConnection _database;
    private readonly Lazy<IReadOnlyDictionary<string, DailyChallenge>> _bundledChallenges;
    private bool _initialized;

    public SqliteChallengeRepository()
    {
        var databasePath = Path.Combine(FileSystem.AppDataDirectory, "improveyourself.db");
        _database = new SQLiteConnection(databasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex);
        _bundledChallenges = new Lazy<IReadOnlyDictionary<string, DailyChallenge>>(LoadBundledChallenges);
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

        _database.Execute("CREATE INDEX IF NOT EXISTS idx_daily_challenges_date ON daily_challenges (date);");
        _database.Execute("CREATE INDEX IF NOT EXISTS idx_challenge_steps_challenge ON challenge_steps (daily_challenge_id, sort_order);");
        SeedBundledChallenges();

        _initialized = true;
    }

    public DailyChallenge? GetChallengeByDate(string date)
    {
        Initialize();

        var row = _database.Table<DailyChallengeRecord>().FirstOrDefault(record => record.Date == date);

        return row is null ? null : MapChallengeRow(row);
    }

    public DailyChallenge GetOrCreateChallenge(string date)
    {
        Initialize();

        var existing = GetChallengeByDate(date);

        if (existing is not null)
        {
            return existing;
        }

        var challenge = GetBundledChallenge(date) ?? ChallengeFactory.CreateDailyChallenge(date);
        InsertChallenge(challenge);

        return GetChallengeByDate(date) ?? challenge;
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
        var completedAt = nextStatus == StepStatus.Completed
            ? DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)
            : null;

        _database.Execute(
            "UPDATE challenge_steps SET status = ?, completed_at = ? WHERE id = ?",
            nextStatus.ToStorage(),
            completedAt,
            targetStep.Id);

        return GetOrCreateChallenge(date);
    }

    public IReadOnlyList<string> ListCompletedDates()
    {
        Initialize();

        var rows = _database.Query<DailyChallengeRecord>(
            "SELECT id, date, title, status, created_at FROM daily_challenges WHERE status = ? ORDER BY date ASC",
            ChallengeStatus.Completed.ToStorage());

        return rows.Select(row => row.Date).ToList();
    }

    public IReadOnlyList<DailyChallenge> ListChallengesBetween(string startDate, string endDate)
    {
        Initialize();

        var rows = _database.Query<DailyChallengeRecord>(
            "SELECT id, date, title, status, created_at FROM daily_challenges WHERE date BETWEEN ? AND ? ORDER BY date ASC",
            startDate,
            endDate);

        return rows.Select(MapChallengeRow).ToList();
    }

    public int GetCompletedChallengesCount()
    {
        Initialize();

        return _database.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM daily_challenges WHERE status = ?",
            ChallengeStatus.Completed.ToStorage());
    }

    private DailyChallenge? GetBundledChallenge(string date)
    {
        if (!_bundledChallenges.Value.TryGetValue(date, out var challenge))
        {
            return null;
        }

        return CloneChallenge(challenge);
    }

    private void SeedBundledChallenges()
    {
        foreach (var challenge in _bundledChallenges.Value.Values)
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
            }
        }
    }

    private DailyChallenge? TryGetStoredChallengeByDate(string date)
    {
        var row = _database.Table<DailyChallengeRecord>().FirstOrDefault(record => record.Date == date);

        return row is null ? null : MapChallengeRow(row);
    }

    private static bool IsPristineChallenge(DailyChallenge challenge) =>
        challenge.Status == ChallengeStatus.NotStarted
        && challenge.Steps.All(step => step.Status == StepStatus.NotStarted);

    private void ReplaceChallenge(DailyChallenge existingChallenge, DailyChallenge bundledChallenge)
    {
        _database.Execute("DELETE FROM challenge_steps WHERE daily_challenge_id = ?", existingChallenge.Id);
        _database.Execute("DELETE FROM daily_challenges WHERE id = ?", existingChallenge.Id);
        InsertChallenge(bundledChallenge);
    }

    private static IReadOnlyDictionary<string, DailyChallenge> LoadBundledChallenges()
    {
        try
        {
            using var stream = FileSystem.OpenAppPackageFileAsync(BundledChallengeFileName).GetAwaiter().GetResult();
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

        var steps = challenge.Steps
            .OrderBy(step => step.SortOrder)
            .Select((step, index) => NormalizeStep(step, challengeId, index + 1))
            .ToList();

        return new DailyChallenge
        {
            Id = challengeId,
            Date = challenge.Date,
            Title = string.IsNullOrWhiteSpace(challenge.Title) ? "Твой ежедневный вызов" : challenge.Title,
            Status = ProgressCalculator.GetChallengeStatus(steps),
            CreatedAt = string.IsNullOrWhiteSpace(challenge.CreatedAt)
                ? DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)
                : challenge.CreatedAt,
            Steps = steps,
        };
    }

    private static ChallengeStep NormalizeStep(ChallengeStep step, string challengeId, int fallbackSortOrder) => new()
    {
        Id = string.IsNullOrWhiteSpace(step.Id)
            ? $"{challengeId}-{step.Type.ToString().ToLowerInvariant()}"
            : step.Id,
        DailyChallengeId = string.IsNullOrWhiteSpace(step.DailyChallengeId) ? challengeId : step.DailyChallengeId,
        Type = step.Type,
        Title = step.Title,
        Subtitle = step.Subtitle,
        Description = step.Description,
        Tip = step.Tip,
        DurationSeconds = step.DurationSeconds,
        QuoteText = step.QuoteText,
        QuoteAuthor = step.QuoteAuthor,
        SortOrder = step.SortOrder <= 0 ? fallbackSortOrder : step.SortOrder,
        Status = step.Status,
        CompletedAt = step.CompletedAt,
    };

    private static DailyChallenge CloneChallenge(DailyChallenge challenge) => new()
    {
        Id = challenge.Id,
        Date = challenge.Date,
        Title = challenge.Title,
        Status = challenge.Status,
        CreatedAt = challenge.CreatedAt,
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
        SortOrder = step.SortOrder,
        Status = step.Status,
        CompletedAt = step.CompletedAt,
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
        SortOrder = row.SortOrder,
        Status = row.Status.ToStepStatus(),
        CompletedAt = row.CompletedAt,
    };

    private DailyChallenge MapChallengeRow(DailyChallengeRecord row)
    {
        var challenge = new DailyChallenge
        {
            Id = row.Id,
            Date = row.Date,
            Title = row.Title,
            Status = row.Status.ToChallengeStatus(),
            CreatedAt = row.CreatedAt,
            Steps = LoadChallengeSteps(row.Id),
        };

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
            _database.Execute(
                "UPDATE daily_challenges SET status = ? WHERE id = ?",
                status.ToStorage(),
                challenge.Id);

            challenge.Status = status;
        }

        return challenge;
    }

    private void InsertChallenge(DailyChallenge challenge)
    {
        _database.Execute(
            "INSERT OR IGNORE INTO daily_challenges (id, date, title, status, created_at) VALUES (?, ?, ?, ?, ?)",
            challenge.Id,
            challenge.Date,
            challenge.Title,
            challenge.Status.ToStorage(),
            challenge.CreatedAt);

        foreach (var step in challenge.Steps)
        {
            _database.Execute(
                "INSERT OR IGNORE INTO challenge_steps (id, daily_challenge_id, type, title, subtitle, description, tip, duration_seconds, quote_text, quote_author, sort_order, status, completed_at) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
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
                step.SortOrder,
                step.Status.ToStorage(),
                step.CompletedAt);
        }
    }
}
