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

public interface IBackendSyncService
{
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
                _ => AppStrings.BackendUnavailable,
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
                    stats.TotalChallengesCompleted,
                    stats.TotalStepsCompleted);

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

    private static SyncDailyChallengeDto MapChallenge(string clientId, DailyChallenge challenge) => new(
        CreateStableGuid(clientId, challenge.Id),
        challenge.Date,
        challenge.Title,
        challenge.Status,
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
            ? timestamp
            : DateTimeOffset.UtcNow;

    private static Guid CreateStableGuid(string clientId, string localId)
    {
        var input = $"{clientId}:{localId}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        var bytes = hash.Take(16).ToArray();

        return new Guid(bytes);
    }

    private sealed record SyncChallengesRequest(
        string ClientId,
        List<SyncDailyChallengeDto> Challenges);

    private sealed record SyncDailyChallengeDto(
        Guid Id,
        string Date,
        string Title,
        ChallengeStatus Status,
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
