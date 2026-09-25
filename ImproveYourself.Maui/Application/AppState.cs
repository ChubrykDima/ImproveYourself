using System.ComponentModel;
using System.Runtime.CompilerServices;
using ImproveYourself.Maui;
using ImproveYourself.Maui.Domain;
using ImproveYourself.Maui.Persistence;
using ImproveYourself.Maui.Resources.Strings;

namespace ImproveYourself.Maui.Application;

public sealed class AppState : INotifyPropertyChanged
{
    private static readonly TimeSpan BackendSyncCooldown = TimeSpan.FromMinutes(3);

    private readonly IChallengeRepository _challengeRepository;
    private readonly ISettingsService _settingsService;
    private readonly INotificationPreferenceService _notificationPreferenceService;
    private readonly IBackendSyncService _backendSyncService;
    private readonly IAnalyticsClient _analyticsClient;
    private readonly IAuthService _authService;

    private bool _isHydrated;
    private bool _isBackendSyncing;
    private string _displayName = string.Empty;
    private bool _onboardingCompleted;
    private bool _notificationsEnabled;
    private string _backendBaseUrl = string.Empty;
    private string _backendSyncMessage = string.Empty;
    private int _currentProgramDay;
    private DailyChallenge? _todayChallenge;
    private BackendStatsSnapshot? _backendStats;
    private StreakSnapshot _streakSnapshot = StreakSnapshot.Empty;
    private MonthlyProgress _monthlyProgress = MonthlyProgress.Empty;
    private WeeklyStats _weeklyStats = WeeklyStats.Empty;
    private List<string> _completedDates = [];
    private SelfAssessmentSnapshot? _startSelfAssessment;
    private SelfAssessmentSnapshot? _finalSelfAssessment;
    private DateTimeOffset? _lastBackendSyncUtc;

    public AppState(
        IChallengeRepository challengeRepository,
        ISettingsService settingsService,
        INotificationPreferenceService notificationPreferenceService,
        IBackendSyncService backendSyncService,
        IAnalyticsClient analyticsClient,
        IAuthService authService)
    {
        _challengeRepository = challengeRepository;
        _settingsService = settingsService;
        _notificationPreferenceService = notificationPreferenceService;
        _backendSyncService = backendSyncService;
        _analyticsClient = analyticsClient;
        _authService = authService;
        _authService.AuthStateChanged += (_, _) => NotifyAuthProperties();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsHydrated
    {
        get => _isHydrated;
        private set => SetProperty(ref _isHydrated, value);
    }

    public bool IsBackendSyncing
    {
        get => _isBackendSyncing;
        private set => SetProperty(ref _isBackendSyncing, value);
    }

    public string DisplayName
    {
        get => _displayName;
        private set => SetProperty(ref _displayName, value);
    }

    public bool OnboardingCompleted
    {
        get => _onboardingCompleted;
        private set => SetProperty(ref _onboardingCompleted, value);
    }

    public bool NotificationsEnabled
    {
        get => _notificationsEnabled;
        private set => SetProperty(ref _notificationsEnabled, value);
    }

    public string BackendBaseUrl
    {
        get => _backendBaseUrl;
        private set => SetProperty(ref _backendBaseUrl, value);
    }

    public bool IsLoggedIn => _authService.IsLoggedIn;

    public string UserEmail => _authService.UserEmail ?? string.Empty;

    public string BackendSyncMessage
    {
        get => _backendSyncMessage;
        private set => SetProperty(ref _backendSyncMessage, value);
    }

    public int CurrentProgramDay
    {
        get => _currentProgramDay;
        private set => SetProperty(ref _currentProgramDay, value);
    }

    public DailyChallenge? TodayChallenge
    {
        get => _todayChallenge;
        private set => SetProperty(ref _todayChallenge, value);
    }

    public BackendStatsSnapshot? BackendStats
    {
        get => _backendStats;
        private set => SetProperty(ref _backendStats, value);
    }

    public StreakSnapshot StreakSnapshot
    {
        get => _streakSnapshot;
        private set => SetProperty(ref _streakSnapshot, value);
    }

    public MonthlyProgress MonthlyProgress
    {
        get => _monthlyProgress;
        private set => SetProperty(ref _monthlyProgress, value);
    }

    public WeeklyStats WeeklyStats
    {
        get => _weeklyStats;
        private set => SetProperty(ref _weeklyStats, value);
    }

    public IReadOnlyList<string> CompletedDates => _completedDates;

    public SelfAssessmentSnapshot? StartSelfAssessment
    {
        get => _startSelfAssessment;
        private set => SetProperty(ref _startSelfAssessment, value);
    }

    public SelfAssessmentSnapshot? FinalSelfAssessment
    {
        get => _finalSelfAssessment;
        private set => SetProperty(ref _finalSelfAssessment, value);
    }

    public bool HasStartSelfAssessment => StartSelfAssessment is not null;

    public bool ShouldShowStartSelfAssessment =>
        OnboardingCompleted
        && StartSelfAssessment is null
        && CompletedDates.Count == 0;

    public bool ShouldShowFinalSelfAssessment =>
        StartSelfAssessment is not null
        && FinalSelfAssessment is null
        && CompletedDates.Count >= DateHelpers.TargetMonthlyDays;

    public Task InitializeAsync()
    {
        _challengeRepository.Initialize();
        _challengeRepository.ReloadBundledContent();

        OnboardingCompleted = _settingsService.ReadOnboardingCompleted();
        DisplayName = _settingsService.ReadDisplayName();
        NotificationsEnabled = _settingsService.ReadNotificationsEnabled();
        BackendBaseUrl = _settingsService.ReadBackendBaseUrl();
        NotifyAuthProperties();
        StartSelfAssessment = _settingsService.ReadSelfAssessment(SelfAssessmentKind.Start);
        FinalSelfAssessment = _settingsService.ReadSelfAssessment(SelfAssessmentKind.Final);

        if (StartSelfAssessment is not null)
        {
            _challengeRepository.ApplyPersonalization(StartSelfAssessment);
        }

        SetCurrentProgramDayInternal(ResolveCurrentProgramDay());

        IsHydrated = true;

        _ = RefreshRemindersAsync();

        return Task.CompletedTask;
    }

    public Task CompleteOnboardingAsync(string name)
    {
        var nextName = name?.Trim() ?? string.Empty;

        _settingsService.WriteDisplayName(nextName);
        _settingsService.WriteOnboardingCompleted(true);

        DisplayName = nextName;
        OnboardingCompleted = true;
        TrackAnalytics(AnalyticsEventNames.OnboardingCompleted);

        return Task.CompletedTask;
    }

    public DailyChallenge EnsureChallengeForProgramDay(int programDay)
    {
        var challenge = _challengeRepository.GetChallengeForProgramDay(programDay, StartSelfAssessment);

        if (programDay == CurrentProgramDay)
        {
            TodayChallenge = challenge;
        }

        return challenge;
    }

    public void TrackChallengeOpened(DailyChallenge challenge)
    {
        TrackAnalytics(
            AnalyticsEventNames.ChallengeOpened,
            new Dictionary<string, string>
            {
                ["program_day"] = challenge.ProgramDayNumber.ToString(),
                ["challenge_status"] = challenge.Status.ToString().ToLowerInvariant(),
            });
    }

    public DailyChallenge SetCurrentProgramDay(int programDay)
    {
        var nextDay = Math.Clamp(programDay, 1, DateHelpers.TargetMonthlyDays);
        SetCurrentProgramDayInternal(nextDay);

        return TodayChallenge ?? _challengeRepository.GetChallengeForProgramDay(nextDay, StartSelfAssessment);
    }

    public DailyChallenge AdvanceToNextDay()
    {
        var referenceDay = CurrentProgramDay == 0 ? ResolveCurrentProgramDay() : CurrentProgramDay;

        if (TodayChallenge is not null
            && TodayChallenge.ProgramDayNumber == referenceDay
            && TodayChallenge.Status != ChallengeStatus.Completed)
        {
            return TodayChallenge;
        }

        if (referenceDay >= DateHelpers.TargetMonthlyDays)
        {
            return TodayChallenge ?? SetCurrentProgramDay(referenceDay);
        }

        return SetCurrentProgramDay(referenceDay + 1);
    }

    public DailyChallenge AdvanceStep(int programDay, StepType stepType)
    {
        var before = _challengeRepository.GetChallengeForProgramDay(programDay, StartSelfAssessment);
        var previousChallengeStatus = before.Status;
        var previousStepStatus = before.Steps.FirstOrDefault(step => step.Type == stepType)?.Status;
        var updated = _challengeRepository.AdvanceChallengeStepStatus(before.Date, stepType);
        updated.ProgramDayNumber = programDay;
        var updatedStepStatus = updated.Steps.FirstOrDefault(step => step.Type == stepType)?.Status;
        var programDayValue = programDay.ToString();
        TrackStepTransition(programDayValue, stepType, previousStepStatus, updatedStepStatus);
        TrackChallengeTransition(programDayValue, previousChallengeStatus, updated.Status);
        var referenceDay = CurrentProgramDay == 0 ? ResolveCurrentProgramDay() : CurrentProgramDay;
        var isCurrentChallenge = programDay == referenceDay;

        if (isCurrentChallenge)
        {
            TodayChallenge = updated;

        }

        LoadDerived();
        TodayChallenge = isCurrentChallenge
            ? updated
            : (TodayChallenge is not null && TodayChallenge.ProgramDayNumber == referenceDay
                ? TodayChallenge
                : _challengeRepository.GetChallengeForProgramDay(referenceDay, StartSelfAssessment));

        return updated;
    }

    public void RefreshDerivedState()
    {
        SetCurrentProgramDayInternal(ResolveCurrentProgramDay());
    }

    public void ReloadAfterLanguageChange()
    {
        _challengeRepository.ReloadBundledContent();
        _challengeRepository.RelocalizeChallenges(StartSelfAssessment);

        RefreshDerivedState();

        _ = RefreshRemindersAsync();
    }

    public async Task<bool> SetNotificationsEnabledAsync(bool enabled)
    {
        var applied = await _notificationPreferenceService.ApplyPreferenceAsync(enabled);

        _settingsService.WriteNotificationsEnabled(applied);
        NotificationsEnabled = applied;

        if (applied)
        {
            TrackAnalytics(
                AnalyticsEventNames.NotificationEnabled,
                new Dictionary<string, string>
                {
                    ["notifications_enabled"] = "true",
                });
        }

        return applied;
    }

    private async Task RefreshRemindersAsync()
    {
        if (!NotificationsEnabled)
        {
            return;
        }

        try
        {
            var active = await _notificationPreferenceService.RefreshAsync(enabled: true);
            if (!active)
            {
                _settingsService.WriteNotificationsEnabled(false);
                MainThread.BeginInvokeOnMainThread(() => NotificationsEnabled = false);
            }
        }
        catch
        {
            // Reminder scheduling must never block startup or language changes.
        }
    }

    public void UpdateDisplayName(string name)
    {
        var nextName = name?.Trim() ?? string.Empty;

        _settingsService.WriteDisplayName(nextName);
        DisplayName = nextName;
    }

    public void UpdateBackendBaseUrl(string baseUrl)
    {
        _settingsService.WriteBackendBaseUrl(baseUrl);
        BackendBaseUrl = _settingsService.ReadBackendBaseUrl();
        BackendSyncMessage = string.IsNullOrWhiteSpace(BackendBaseUrl)
            ? AppStrings.BackendNotConfigured
            : AppStrings.BackendSaved;
    }

    public Task<AuthOperationResult> RegisterAsync(string email, string password, CancellationToken cancellationToken = default) =>
        _authService.RegisterAsync(email, password, cancellationToken);

    public Task<AuthOperationResult> LoginAsync(string email, string password, CancellationToken cancellationToken = default) =>
        _authService.LoginAsync(email, password, cancellationToken);

    public Task<AuthOperationResult> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default) =>
        _authService.RequestPasswordResetAsync(email, cancellationToken);

    public Task<AuthOperationResult> ConfirmPasswordResetAsync(
        string email,
        string token,
        string newPassword,
        CancellationToken cancellationToken = default) =>
        _authService.ConfirmPasswordResetAsync(email, token, newPassword, cancellationToken);

    public Task<AccountExportResult> ExportAccountAsync(CancellationToken cancellationToken = default) =>
        _authService.ExportAccountAsync(cancellationToken);

    public Task<AuthOperationResult> DeleteAccountAsync(CancellationToken cancellationToken = default) =>
        _authService.DeleteAccountAsync(cancellationToken);

    public Task LogoutAsync(CancellationToken cancellationToken = default) =>
        _authService.LogoutAsync(cancellationToken);

    public async Task<BackendSyncResult> SyncBackendAsync(bool force = false, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(BackendBaseUrl))
        {
            return new BackendSyncResult(false, false, AppStrings.BackendProvideUrl, BackendStats);
        }

        if (!_authService.IsLoggedIn)
        {
            return new BackendSyncResult(true, false, AppStrings.AuthLoginRequired, BackendStats);
        }

        if (!force
            && _lastBackendSyncUtc is not null
            && DateTimeOffset.UtcNow - _lastBackendSyncUtc.Value < BackendSyncCooldown)
        {
            return new BackendSyncResult(
                true,
                true,
                AppStrings.BackendSyncRecently,
                BackendStats);
        }

        if (IsBackendSyncing)
        {
            return new BackendSyncResult(
                !string.IsNullOrWhiteSpace(BackendBaseUrl),
                false,
                AppStrings.BackendSyncInProgress,
                BackendStats);
        }

        IsBackendSyncing = true;

        try
        {
            var challenges = _challengeRepository.ListAllChallenges();
            var result = await _backendSyncService.SyncChallengesAsync(challenges, cancellationToken);
            _lastBackendSyncUtc = DateTimeOffset.UtcNow;

            if (result.IsConfigured || !string.IsNullOrWhiteSpace(BackendBaseUrl))
            {
                BackendSyncMessage = result.Message;
            }

            if (result.Stats is not null)
            {
                BackendStats = result.Stats;
            }

            TrackAnalytics(
                result.Succeeded ? AnalyticsEventNames.BackendSyncSucceeded : AnalyticsEventNames.BackendSyncFailed,
                new Dictionary<string, string>
                {
                    ["configured"] = result.IsConfigured ? "true" : "false",
                    ["has_stats"] = result.Stats is not null ? "true" : "false",
                });

            return result;
        }
        catch (Exception ex)
        {
            var message = string.Format(AppStrings.BackendSyncFailedFormat, ex.Message);
            BackendSyncMessage = message;
            TrackAnalytics(
                AnalyticsEventNames.BackendSyncFailed,
                new Dictionary<string, string>
                {
                    ["configured"] = !string.IsNullOrWhiteSpace(BackendBaseUrl) ? "true" : "false",
                    ["failure_type"] = "exception",
                });

            return new BackendSyncResult(
                !string.IsNullOrWhiteSpace(BackendBaseUrl),
                false,
                message,
                BackendStats);
        }
        finally
        {
            IsBackendSyncing = false;
        }
    }

    public void SaveSelfAssessment(SelfAssessmentSnapshot snapshot)
    {
        _settingsService.WriteSelfAssessment(snapshot);
        TrackAnalytics(
            AnalyticsEventNames.SelfAssessmentCompleted,
            new Dictionary<string, string>
            {
                ["kind"] = snapshot.Kind.ToString().ToLowerInvariant(),
            });

        if (snapshot.Kind == SelfAssessmentKind.Start)
        {
            StartSelfAssessment = snapshot;
            _challengeRepository.ApplyPersonalization(snapshot);

            if (CurrentProgramDay > 0)
            {
                TodayChallenge = _challengeRepository.GetChallengeForProgramDay(CurrentProgramDay, snapshot);
            }

            OnPropertyChanged(nameof(HasStartSelfAssessment));
            OnPropertyChanged(nameof(ShouldShowStartSelfAssessment));
            OnPropertyChanged(nameof(ShouldShowFinalSelfAssessment));
            return;
        }

        FinalSelfAssessment = snapshot;
        OnPropertyChanged(nameof(ShouldShowFinalSelfAssessment));
    }

    private int ResolveCurrentProgramDay()
    {
        var storedValue = CurrentProgramDay > 0
            ? CurrentProgramDay.ToString()
            : _settingsService.ReadCurrentProgramDay();

        if (int.TryParse(storedValue, out var storedDay)
            && storedDay >= 1
            && storedDay <= DateHelpers.TargetMonthlyDays)
        {
            return storedDay;
        }

        // Versions before program days stored a calendar date here. Preserve their completed
        // work by mapping it to the next sequential day of the new program.
        var completedLegacyChallenges = _challengeRepository.GetCompletedChallengesCount();
        return Math.Clamp(completedLegacyChallenges + 1, 1, DateHelpers.TargetMonthlyDays);
    }

    private void SetCurrentProgramDayInternal(int programDay)
    {
        CurrentProgramDay = programDay;
        _settingsService.WriteCurrentProgramDay(programDay);
        TodayChallenge = _challengeRepository.GetChallengeForProgramDay(programDay, StartSelfAssessment);
        LoadDerived();
    }

    private void LoadDerived()
    {
        var completedDates = _challengeRepository.ListCompletedDates().ToList();
        var todayIsoDate = DateHelpers.ToIsoDate(DateTime.UtcNow);
        var streakSnapshot = ProgressCalculator.CalculateStreakSnapshot(completedDates, todayIsoDate);
        var completedProgramDays = Math.Min(completedDates.Count, DateHelpers.TargetMonthlyDays);
        var monthlyProgress = new MonthlyProgress
        {
            CompletedDays = completedProgramDays,
            TargetDays = DateHelpers.TargetMonthlyDays,
            Percent = (int)Math.Round(completedProgramDays / (double)DateHelpers.TargetMonthlyDays * 100, MidpointRounding.AwayFromZero),
            RemainingDays = Math.Max(DateHelpers.TargetMonthlyDays - completedProgramDays, 0),
        };

        var weekStart = DateHelpers.ParseIsoDate(todayIsoDate).AddDays(-6).ToString("yyyy-MM-dd");
        var weeklyChallenges = _challengeRepository.ListChallengesBetween(weekStart, todayIsoDate);
        var weeklyStats = ProgressCalculator.BuildWeeklyStats(weeklyChallenges, todayIsoDate);

        _completedDates = completedDates;
        OnPropertyChanged(nameof(CompletedDates));
        OnPropertyChanged(nameof(ShouldShowStartSelfAssessment));
        OnPropertyChanged(nameof(ShouldShowFinalSelfAssessment));

        StreakSnapshot = streakSnapshot;
        MonthlyProgress = monthlyProgress;
        WeeklyStats = weeklyStats;
    }

    private void TrackStepTransition(
        string programDay,
        StepType stepType,
        StepStatus? previousStatus,
        StepStatus? updatedStatus)
    {
        if (previousStatus is null || updatedStatus is null || previousStatus == updatedStatus)
        {
            return;
        }

        var eventName = updatedStatus switch
        {
            StepStatus.InProgress => AnalyticsEventNames.StepStarted,
            StepStatus.Completed => AnalyticsEventNames.StepCompleted,
            _ => string.Empty,
        };

        if (string.IsNullOrWhiteSpace(eventName))
        {
            return;
        }

        TrackAnalytics(
            eventName,
            new Dictionary<string, string>
            {
                ["program_day"] = programDay,
                ["step_type"] = stepType.ToString().ToLowerInvariant(),
                ["step_status"] = updatedStatus.Value.ToString().ToLowerInvariant(),
            });
    }

    private void TrackChallengeTransition(
        string programDay,
        ChallengeStatus previousStatus,
        ChallengeStatus updatedStatus)
    {
        if (previousStatus == ChallengeStatus.Completed || updatedStatus != ChallengeStatus.Completed)
        {
            return;
        }

        TrackAnalytics(
            AnalyticsEventNames.ChallengeCompleted,
            new Dictionary<string, string>
            {
                ["program_day"] = programDay,
                ["challenge_status"] = updatedStatus.ToString().ToLowerInvariant(),
            });
    }

    private void TrackAnalytics(string eventName, IReadOnlyDictionary<string, string>? properties = null)
    {
        _ = _analyticsClient.TrackAsync(eventName, properties);
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);

        return true;
    }

    private void NotifyAuthProperties()
    {
        OnPropertyChanged(nameof(IsLoggedIn));
        OnPropertyChanged(nameof(UserEmail));
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
