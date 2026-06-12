using System.ComponentModel;
using System.Runtime.CompilerServices;
using ImproveYourself.Maui.Domain;
using ImproveYourself.Maui.Persistence;

namespace ImproveYourself.Maui.Application;

public sealed class AppState : INotifyPropertyChanged
{
    private readonly IChallengeRepository _challengeRepository;
    private readonly ISettingsService _settingsService;
    private readonly INotificationPreferenceService _notificationPreferenceService;

    private bool _isHydrated;
    private string _displayName = "Друг";
    private bool _onboardingCompleted;
    private bool _notificationsEnabled;
    private string _currentChallengeDate = string.Empty;
    private DailyChallenge? _todayChallenge;
    private StreakSnapshot _streakSnapshot = StreakSnapshot.Empty;
    private MonthlyProgress _monthlyProgress = MonthlyProgress.Empty;
    private WeeklyStats _weeklyStats = WeeklyStats.Empty;
    private List<string> _completedDates = [];

    public AppState(
        IChallengeRepository challengeRepository,
        ISettingsService settingsService,
        INotificationPreferenceService notificationPreferenceService)
    {
        _challengeRepository = challengeRepository;
        _settingsService = settingsService;
        _notificationPreferenceService = notificationPreferenceService;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsHydrated
    {
        get => _isHydrated;
        private set => SetProperty(ref _isHydrated, value);
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

    public string CurrentChallengeDate
    {
        get => _currentChallengeDate;
        private set => SetProperty(ref _currentChallengeDate, value);
    }

    public DailyChallenge? TodayChallenge
    {
        get => _todayChallenge;
        private set => SetProperty(ref _todayChallenge, value);
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

    public Task InitializeAsync()
    {
        _challengeRepository.Initialize();

        OnboardingCompleted = _settingsService.ReadOnboardingCompleted();
        DisplayName = _settingsService.ReadDisplayName();
        NotificationsEnabled = _settingsService.ReadNotificationsEnabled();

        SetCurrentChallengeDateInternal(ResolveCurrentChallengeDate());

        IsHydrated = true;

        return Task.CompletedTask;
    }

    public Task CompleteOnboardingAsync(string name)
    {
        var nextName = string.IsNullOrWhiteSpace(name) ? "Друг" : name.Trim();

        _settingsService.WriteDisplayName(nextName);
        _settingsService.WriteOnboardingCompleted(true);

        DisplayName = nextName;
        OnboardingCompleted = true;

        return Task.CompletedTask;
    }

    public DailyChallenge EnsureChallengeForDate(string date)
    {
        var challenge = _challengeRepository.GetOrCreateChallenge(date);

        if (date == CurrentChallengeDate)
        {
            TodayChallenge = challenge;
        }

        return challenge;
    }

    public DailyChallenge SetCurrentChallengeDate(string date)
    {
        var nextDate = ResolveRequestedChallengeDate(date);
        SetCurrentChallengeDateInternal(nextDate);

        return TodayChallenge ?? _challengeRepository.GetOrCreateChallenge(nextDate);
    }

    public DailyChallenge AdvanceToNextDay()
    {
        var referenceDate = string.IsNullOrWhiteSpace(CurrentChallengeDate)
            ? ResolveCurrentChallengeDate()
            : CurrentChallengeDate;

        if (TodayChallenge is not null
            && TodayChallenge.Date == referenceDate
            && TodayChallenge.Status != ChallengeStatus.Completed)
        {
            return TodayChallenge;
        }

        return SetCurrentChallengeDate(DateHelpers.AddDays(referenceDate, 1));
    }

    public DailyChallenge AdvanceStep(string date, StepType stepType)
    {
        var updated = _challengeRepository.AdvanceChallengeStepStatus(date, stepType);
        // Use CurrentChallengeDate directly to avoid re-evaluating against the DB after the
        // step update (which may have just completed the challenge, causing ResolveChallengeDate
        // to return today's date instead of the active challenge date).
        var referenceDate = string.IsNullOrWhiteSpace(CurrentChallengeDate)
            ? ResolveCurrentChallengeDate()
            : CurrentChallengeDate;
        var isCurrentChallenge = date == referenceDate;

        if (isCurrentChallenge)
        {
            TodayChallenge = updated;

            if (updated.Status == ChallengeStatus.Completed)
            {
                return AdvanceToNextDay();
            }
        }

        LoadDerived(referenceDate);
        TodayChallenge = isCurrentChallenge
            ? updated
            : (TodayChallenge is not null && TodayChallenge.Date == referenceDate
                ? TodayChallenge
                : _challengeRepository.GetOrCreateChallenge(referenceDate));

        return updated;
    }

    public void RefreshDerivedState()
    {
        SetCurrentChallengeDateInternal(ResolveCurrentChallengeDate());
    }

    public async Task<bool> SetNotificationsEnabledAsync(bool enabled)
    {
        var applied = await _notificationPreferenceService.ApplyPreferenceAsync(enabled);

        _settingsService.WriteNotificationsEnabled(applied);
        NotificationsEnabled = applied;

        return applied;
    }

    public void UpdateDisplayName(string name)
    {
        var nextName = string.IsNullOrWhiteSpace(name) ? "Друг" : name.Trim();

        _settingsService.WriteDisplayName(nextName);
        DisplayName = nextName;
    }

    private string ResolveCurrentChallengeDate()
    {
        var todayIsoDate = DateHelpers.ToIsoDate(DateTime.Now);
        var preferredDate = string.IsNullOrWhiteSpace(CurrentChallengeDate)
            ? _settingsService.ReadCurrentChallengeDate()
            : CurrentChallengeDate;

        return ResolveChallengeDate(preferredDate, todayIsoDate);
    }

    private string ResolveRequestedChallengeDate(string date)
    {
        if (DateHelpers.TryParseIsoDate(date, out _))
        {
            return date;
        }

        return ResolveCurrentChallengeDate();
    }

    private string ResolveChallengeDate(string preferredDate, string fallbackDate)
    {
        if (!DateHelpers.TryParseIsoDate(preferredDate, out _))
        {
            return ResolveNextIncompleteDate(fallbackDate);
        }

        string candidateDate;

        if (string.CompareOrdinal(preferredDate, fallbackDate) >= 0)
        {
            candidateDate = preferredDate;
        }
        else
        {
            var preferredChallenge = _challengeRepository.GetChallengeByDate(preferredDate);

            candidateDate = preferredChallenge is not null && preferredChallenge.Status != ChallengeStatus.Completed
                ? preferredDate
                : fallbackDate;
        }

        return ResolveNextIncompleteDate(candidateDate);
    }

    private string ResolveNextIncompleteDate(string startDate)
    {
        var date = startDate;

        while (true)
        {
            var challenge = _challengeRepository.GetChallengeByDate(date);

            if (challenge is null || challenge.Status != ChallengeStatus.Completed)
            {
                return date;
            }

            date = DateHelpers.AddDays(date, 1);
        }
    }

    private void SetCurrentChallengeDateInternal(string date)
    {
        CurrentChallengeDate = date;
        _settingsService.WriteCurrentChallengeDate(date);
        TodayChallenge = _challengeRepository.GetOrCreateChallenge(date);
        LoadDerived(date);
    }

    private void LoadDerived(string todayIsoDate)
    {
        var completedDates = _challengeRepository.ListCompletedDates().ToList();
        var streakSnapshot = ProgressCalculator.CalculateStreakSnapshot(completedDates, todayIsoDate);
        var monthlyProgress = ProgressCalculator.CalculateMonthlyProgress(
            completedDates,
            DateHelpers.ParseIsoDate(todayIsoDate).ToDateTime(TimeOnly.MinValue));

        var weekStart = DateHelpers.ParseIsoDate(todayIsoDate).AddDays(-6).ToString("yyyy-MM-dd");
        var weeklyChallenges = _challengeRepository.ListChallengesBetween(weekStart, todayIsoDate);
        var weeklyStats = ProgressCalculator.BuildWeeklyStats(weeklyChallenges, todayIsoDate);

        _completedDates = completedDates;
        OnPropertyChanged(nameof(CompletedDates));

        StreakSnapshot = streakSnapshot;
        MonthlyProgress = monthlyProgress;
        WeeklyStats = weeklyStats;
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

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
