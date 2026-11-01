using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using ImproveYourself.Maui.Persistence;
using ImproveYourself.Maui.Resources.Strings;
using ImproveYourself.Maui.Domain;

namespace ImproveYourself.Maui.Application;

public sealed record BackendConnectionResult(
    bool IsConfigured,
    bool HealthOk,
    bool ReadyOk,
    bool AuthorizationOk,
    string Message);

public interface IBackendConnectionService
{
    Task<BackendConnectionResult> CheckAsync(CancellationToken cancellationToken = default);
}

public sealed record BackendStatsSnapshot(
    int TotalChallengesCompleted,
    int TotalStepsCompleted,
    int CurrentStreak);

public sealed record BackendSyncResult(
    bool IsConfigured,
    bool Succeeded,
    string Message,
    BackendStatsSnapshot? Stats = null);

public sealed record RemoteChallengesDownload(
    bool IsConfigured,
    bool Succeeded,
    string Message,
    IReadOnlyList<RemoteProgramDay> Challenges);

public sealed record RemoteAssessmentsDownload(
    bool IsConfigured,
    bool Succeeded,
    string Message,
    SelfAssessmentSnapshot? Start,
    SelfAssessmentSnapshot? Final);

public interface IBackendSyncService
{
    Task<RemoteChallengesDownload> DownloadChallengesAsync(CancellationToken cancellationToken = default);

    Task<RemoteAssessmentsDownload> DownloadSelfAssessmentsAsync(CancellationToken cancellationToken = default);

    Task UploadSelfAssessmentsAsync(
        IReadOnlyList<SelfAssessmentSnapshot> snapshots,
        CancellationToken cancellationToken = default);

    Task<BackendSyncResult> SyncChallengesAsync(
        IReadOnlyList<DailyChallenge> challenges,
        CancellationToken cancellationToken = default);
}

public sealed class BackendConnectionService : IBackendConnectionService, IBackendSyncService
{
    // Backend sync expects numeric enums (not JsonStringEnumConverter strings).
    private static readonly JsonSerializerOptions ApiJsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly ISettingsService _settingsService;
    private readonly IAuthService _authService;

    public BackendConnectionService(
        HttpClient httpClient,
        ISettingsService settingsService,
        IAuthService authService)
    {
        _httpClient = httpClient;
        _settingsService = settingsService;
        _authService = authService;
    }

    public async Task<BackendConnectionResult> CheckAsync(CancellationToken cancellationToken = default)
    {
        var (baseUri, errorMessage) = ReadBackendBaseUri();

        if (baseUri is null)
        {
            return new BackendConnectionResult(false, false, false, false, errorMessage);
        }

        if (!_authService.IsLoggedIn)
        {
            return new BackendConnectionResult(true, false, false, false, AppStrings.AuthLoginRequired);
        }

        try
        {
            // /ready is public. /health and /auth/check require Bearer JWT on production.
            var healthOk = await IsSuccessAsync(baseUri, "health", cancellationToken);
            var readyOk = await IsSuccessAsync(baseUri, "ready", cancellationToken);
            var authStatus = await GetStatusAsync(baseUri, "auth/check", cancellationToken);
            var authorizationOk = authStatus == HttpStatusCode.OK;

            var message = (healthOk, readyOk, authorizationOk, authStatus) switch
            {
                (true, true, true, _) => AppStrings.BackendAllOk,
                (_, true, true, _) => AppStrings.BackendAllOk,
                (true, false, true, _) => AppStrings.BackendNoDb,
                (_, false, true, _) => AppStrings.BackendNoDb,
                (_, _, false, HttpStatusCode.Unauthorized) => AppStrings.AuthSessionExpired,
                (_, _, false, _) => string.Format(AppStrings.BackendBadStatusFormat, (int)authStatus),
            };

            return new BackendConnectionResult(true, healthOk, readyOk, authorizationOk, message);
        }
        catch (TaskCanceledException)
        {
            return new BackendConnectionResult(true, false, false, false, AppStrings.BackendTimeout);
        }
        catch (Exception ex)
        {
            return new BackendConnectionResult(true, false, false, false, string.Format(AppStrings.BackendConnectionFailedFormat, ex.Message));
        }
    }

    public async Task<RemoteChallengesDownload> DownloadChallengesAsync(CancellationToken cancellationToken = default)
    {
        var (baseUri, errorMessage) = ReadBackendBaseUri();

        if (baseUri is null)
        {
            return new RemoteChallengesDownload(false, false, errorMessage, []);
        }

        if (!_authService.IsLoggedIn)
        {
            return new RemoteChallengesDownload(true, false, AppStrings.AuthLoginRequired, []);
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(baseUri, "api/challenges"));
            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new RemoteChallengesDownload(
                    true,
                    false,
                    string.Format(AppStrings.BackendSyncFailedStatusFormat, (int)response.StatusCode),
                    []);
            }

            var challenges = await response.Content.ReadFromJsonAsync<List<SyncDailyChallengeDto>>(ApiJsonOptions, cancellationToken)
                ?? [];

            return new RemoteChallengesDownload(
                true,
                true,
                AppStrings.BackendSyncSucceeded,
                challenges.Select(MapRemoteDay).ToList());
        }
        catch (TaskCanceledException)
        {
            return new RemoteChallengesDownload(true, false, AppStrings.BackendSyncTimeout, []);
        }
        catch (Exception ex)
        {
            return new RemoteChallengesDownload(
                true,
                false,
                string.Format(AppStrings.BackendSyncFailedFormat, ex.Message),
                []);
        }
    }

    public async Task<RemoteAssessmentsDownload> DownloadSelfAssessmentsAsync(CancellationToken cancellationToken = default)
    {
        var (baseUri, errorMessage) = ReadBackendBaseUri();

        if (baseUri is null)
        {
            return new RemoteAssessmentsDownload(false, false, errorMessage, null, null);
        }

        if (!_authService.IsLoggedIn)
        {
            return new RemoteAssessmentsDownload(true, false, AppStrings.AuthLoginRequired, null, null);
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(baseUri, "api/self-assessments"));
            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new RemoteAssessmentsDownload(
                    true,
                    false,
                    string.Format(AppStrings.BackendSyncFailedStatusFormat, (int)response.StatusCode),
                    null,
                    null);
            }

            var payload = await response.Content.ReadFromJsonAsync<SelfAssessmentsResponse>(ApiJsonOptions, cancellationToken);

            return new RemoteAssessmentsDownload(
                true,
                true,
                AppStrings.BackendSyncSucceeded,
                MapAssessment(payload?.Start, SelfAssessmentKind.Start),
                MapAssessment(payload?.Final, SelfAssessmentKind.Final));
        }
        catch (TaskCanceledException)
        {
            return new RemoteAssessmentsDownload(true, false, AppStrings.BackendSyncTimeout, null, null);
        }
        catch (Exception ex)
        {
            return new RemoteAssessmentsDownload(
                true,
                false,
                string.Format(AppStrings.BackendSyncFailedFormat, ex.Message),
                null,
                null);
        }
    }

    public async Task UploadSelfAssessmentsAsync(
        IReadOnlyList<SelfAssessmentSnapshot> snapshots,
        CancellationToken cancellationToken = default)
    {
        var (baseUri, _) = ReadBackendBaseUri();

        if (baseUri is null || !_authService.IsLoggedIn)
        {
            return;
        }

        foreach (var snapshot in snapshots)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Put, new Uri(baseUri, "api/self-assessments"))
                {
                    Content = JsonContent.Create(MapAssessmentRequest(snapshot), options: ApiJsonOptions),
                };
                using var response = await _httpClient.SendAsync(request, cancellationToken);
            }
            catch
            {
                // A failed assessment upload must not discard challenge progress already stored locally.
            }
        }
    }

    public async Task<BackendSyncResult> SyncChallengesAsync(
        IReadOnlyList<DailyChallenge> challenges,
        CancellationToken cancellationToken = default)
    {
        var (baseUri, errorMessage) = ReadBackendBaseUri();

        if (baseUri is null)
        {
            return new BackendSyncResult(false, false, errorMessage);
        }

        if (!_authService.IsLoggedIn)
        {
            return new BackendSyncResult(true, false, AppStrings.AuthLoginRequired);
        }

        if (challenges.Count == 0)
        {
            return new BackendSyncResult(true, true, AppStrings.BackendSyncNoChallenges);
        }

        var clientId = _settingsService.ReadBackendClientId();
        var requestBody = new SyncChallengesRequest(
            clientId,
            challenges.Select(challenge => MapChallenge(clientId, challenge)).ToList());

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(baseUri, "api/challenges/sync"))
            {
                Content = JsonContent.Create(requestBody, options: ApiJsonOptions),
            };

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new BackendSyncResult(
                    true,
                    false,
                    string.Format(AppStrings.BackendSyncFailedStatusFormat, (int)response.StatusCode));
            }

            var stats = await GetStatsAsync(baseUri, cancellationToken);
            var message = stats is null
                ? AppStrings.BackendSyncSucceeded
                : string.Format(
                    AppStrings.BackendSyncSucceededWithStatsFormat,
                    AppStrings.Plural("DaysCount", stats.TotalChallengesCompleted),
                    AppStrings.Plural("StepsCount", stats.TotalStepsCompleted));

            return new BackendSyncResult(true, true, message, stats);
        }
        catch (TaskCanceledException)
        {
            return new BackendSyncResult(true, false, AppStrings.BackendSyncTimeout);
        }
        catch (Exception ex)
        {
            return new BackendSyncResult(true, false, string.Format(AppStrings.BackendSyncFailedFormat, ex.Message));
        }
    }

    private async Task<bool> IsSuccessAsync(Uri baseUri, string path, CancellationToken cancellationToken)
    {
        var status = await GetStatusAsync(baseUri, path, cancellationToken);
        return (int)status is >= 200 and <= 299;
    }

    private async Task<HttpStatusCode> GetStatusAsync(Uri baseUri, string path, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(baseUri, path));
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        return response.StatusCode;
    }

    private async Task<BackendStatsSnapshot?> GetStatsAsync(Uri baseUri, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(baseUri, "api/stats"));
        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<BackendStatsSnapshot>(ApiJsonOptions, cancellationToken);
    }

    private (Uri? BaseUri, string ErrorMessage) ReadBackendBaseUri()
    {
        var baseUrl = _settingsService.ReadBackendBaseUrl();

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return (null, AppStrings.BackendProvideUrl);
        }

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri)
            || (baseUri.Scheme != Uri.UriSchemeHttp && baseUri.Scheme != Uri.UriSchemeHttps))
        {
            return (null, AppStrings.BackendInvalidUrl);
        }

        return (baseUri, string.Empty);
    }

    private static RemoteProgramDay MapRemoteDay(SyncDailyChallengeDto challenge) => new()
    {
        ProgramDayNumber = challenge.ProgramDayNumber,
        CompletedAt = FormatTimestamp(challenge.CompletedAt),
        Steps = challenge.Steps.Select(step => new RemoteProgramStep
        {
            Type = step.Type,
            Status = step.Status,
            CompletedAt = FormatTimestamp(step.CompletedAt),
            UpdatedAt = FormatTimestamp(step.UpdatedAt) ?? string.Empty,
        }).ToList(),
    };

    private static SelfAssessmentSnapshot? MapAssessment(SelfAssessmentDto? dto, SelfAssessmentKind kind)
    {
        if (dto is null)
        {
            return null;
        }

        return new SelfAssessmentSnapshot
        {
            Kind = kind,
            CreatedAt = dto.CreatedAt.ToUniversalTime().ToString("o"),
            ConversationStartComfort = ClampScore(dto.ConversationStartComfort),
            OpinionExpressionEase = ClampScore(dto.OpinionExpressionEase),
            BodyCalmUnderStress = ClampScore(dto.BodyCalmUnderStress),
            EyeContactEase = ClampScore(dto.EyeContactEase),
            SocialActionReadiness = ClampScore(dto.SocialActionReadiness),
        };
    }

    private static SelfAssessmentUpsertRequest MapAssessmentRequest(SelfAssessmentSnapshot snapshot)
    {
        var createdAt = DateTimeOffset.TryParse(snapshot.CreatedAt, out var parsed)
            ? parsed.ToUniversalTime()
            : DateTimeOffset.UtcNow;

        return new SelfAssessmentUpsertRequest(
            snapshot.Kind,
            createdAt,
            ClampScore(snapshot.ConversationStartComfort),
            ClampScore(snapshot.OpinionExpressionEase),
            ClampScore(snapshot.BodyCalmUnderStress),
            ClampScore(snapshot.EyeContactEase),
            ClampScore(snapshot.SocialActionReadiness));
    }

    private static int ClampScore(int score) => Math.Clamp(score, 1, 10);

    private static string? FormatTimestamp(DateTimeOffset? value) =>
        value?.ToUniversalTime().ToString("o");

    private static string FormatTimestamp(DateTimeOffset value) =>
        value.ToUniversalTime().ToString("o");

    private static SyncDailyChallengeDto MapChallenge(string clientId, DailyChallenge challenge) => new(
        CreateStableGuid(clientId, challenge.Id),
        challenge.ProgramDayNumber,
        challenge.Title,
        challenge.Status,
        ParseNullableTimestamp(challenge.CompletedAt),
        ParseTimestamp(challenge.CreatedAt),
        challenge.QuoteText,
        challenge.QuoteAuthor,
        ParseTimestamp(challenge.UpdatedAt),
        challenge.Steps.Select(step => MapStep(clientId, step)).ToList());

    private static SyncChallengeStepDto MapStep(string clientId, ChallengeStep step) => new(
        CreateStableGuid(clientId, step.Id),
        step.Type,
        step.Title,
        step.Subtitle,
        step.Description,
        step.Tip,
        step.DurationSeconds,
        step.QuoteText,
        step.QuoteAuthor,
        step.SortOrder,
        step.Status,
        string.IsNullOrWhiteSpace(step.CompletedAt) ? null : ParseTimestamp(step.CompletedAt),
        ParseTimestamp(step.UpdatedAt));

    private static DateTimeOffset ParseTimestamp(string value) =>
        DateTimeOffset.TryParse(value, out var timestamp)
            ? timestamp.ToUniversalTime()
            : DateTimeOffset.UtcNow;

    private static DateTimeOffset? ParseNullableTimestamp(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : ParseTimestamp(value);

    private static Guid CreateStableGuid(string clientId, string localId)
    {
        var input = $"{clientId}:{localId}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        var bytes = hash.Take(16).ToArray();

        return new Guid(bytes);
    }

    private sealed record SelfAssessmentsResponse(SelfAssessmentDto? Start, SelfAssessmentDto? Final);

    private sealed record SelfAssessmentDto(
        SelfAssessmentKind Kind,
        DateTimeOffset CreatedAt,
        int ConversationStartComfort,
        int OpinionExpressionEase,
        int BodyCalmUnderStress,
        int EyeContactEase,
        int SocialActionReadiness);

    private sealed record SelfAssessmentUpsertRequest(
        SelfAssessmentKind Kind,
        DateTimeOffset CreatedAt,
        int ConversationStartComfort,
        int OpinionExpressionEase,
        int BodyCalmUnderStress,
        int EyeContactEase,
        int SocialActionReadiness);

    private sealed record SyncChallengesRequest(
        string ClientId,
        List<SyncDailyChallengeDto> Challenges);

    private sealed record SyncDailyChallengeDto(
        Guid Id,
        int ProgramDayNumber,
        string Title,
        ChallengeStatus Status,
        DateTimeOffset? CompletedAt,
        DateTimeOffset CreatedAt,
        string? QuoteText,
        string? QuoteAuthor,
        DateTimeOffset UpdatedAt,
        List<SyncChallengeStepDto> Steps);

    private sealed record SyncChallengeStepDto(
        Guid Id,
        StepType Type,
        string Title,
        string? Subtitle,
        string Description,
        string? Tip,
        int? DurationSeconds,
        string? QuoteText,
        string? QuoteAuthor,
        int SortOrder,
        StepStatus Status,
        DateTimeOffset? CompletedAt,
        DateTimeOffset UpdatedAt);
}
