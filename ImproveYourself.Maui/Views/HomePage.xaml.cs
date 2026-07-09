using ImproveYourself.Maui.Application;
using ImproveYourself.Maui.Domain;
using ImproveYourself.Maui.Resources.Strings;

namespace ImproveYourself.Maui.Views;

public partial class HomePage : ContentPage
{
    private readonly AppState _appState;
    private readonly IBackendConnectionService _backendConnectionService;
    private readonly ILocalizationService _localizationService;
    private bool _isOpeningFinalAssessment;

    public HomePage(AppState appState, IBackendConnectionService backendConnectionService, ILocalizationService localizationService)
    {
        InitializeComponent();
        _appState = appState;
        _backendConnectionService = backendConnectionService;
        _localizationService = localizationService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _appState.RefreshDerivedState();
        RenderStaticLabels();
        Render();
        _ = SyncBackendAndRenderAsync();
        await TryOpenFinalAssessmentAsync();
    }

    private void RenderStaticLabels()
    {
        Title = AppStrings.HomePage_Title;
        WelcomeLabel.Text = AppStrings.Welcome;
        CurrentStreakLabel.Text = AppStrings.CurrentStreak;
        DaysInRowLabel.Text = AppStrings.DaysInRow;
        MonthProgressLabel.Text = AppStrings.MonthProgress;
        StatisticsButton.Text = $"{AppStrings.StatisticsButtonTitle}\n{AppStrings.StatisticsButtonSubtitle}";
        CalendarButton.Text = $"{AppStrings.CalendarButtonTitle}\n{AppStrings.CalendarButtonSubtitle}";
        SettingsButton.Text = AppStrings.OpenSettings;
    }

    private void Render()
    {
        var name = ShortName(_appState.DisplayName);
        GreetingNameLabel.Text = name;
        AvatarInitialLabel.Text = name[..1].ToUpperInvariant();

        StreakValueLabel.Text = _appState.StreakSnapshot.CurrentStreakDays.ToString();
        MonthPercentLabel.Text = $"{_appState.MonthlyProgress.Percent}%";
        MonthHintLabel.Text = string.Format(AppStrings.MonthProgressFormat, _appState.MonthlyProgress.CompletedDays, _appState.MonthlyProgress.TargetDays);
        RemainingDaysLabel.Text = string.Format(AppStrings.RemainingDaysGoal, _appState.MonthlyProgress.RemainingDays);

        if (_appState.TodayChallenge is null)
        {
            ChallengeLeadLabel.Text = AppStrings.MainFocusOfDay;
            ChallengeTitleLabel.Text = AppStrings.PreparingChallenge;
            ChallengeSubtitleLabel.Text = AppStrings.TryOpenInSecond;
            PersonalizationLabel.IsVisible = false;
            PersonalizationLabel.Text = string.Empty;
            ChallengeProgressBar.Progress = 0;
            ChallengeButton.Text = AppStrings.PleaseWait;
            ChallengeButton.IsEnabled = false;
            return;
        }

        var completedSteps = ProgressCalculator.CountCompletedSteps(_appState.TodayChallenge.Steps);
        var totalSteps = Math.Max(_appState.TodayChallenge.Steps.Count, 1);
        var isCompleted = _appState.TodayChallenge.Status == ChallengeStatus.Completed;
        var isToday = _appState.TodayChallenge.Date == DateHelpers.ToIsoDate(DateTime.Now);

        ChallengeLeadLabel.Text = isToday
            ? AppStrings.MainFocusOfDay
            : string.Format(AppStrings.FocusOnDate, DateHelpers.ToDisplayDate(_appState.TodayChallenge.Date));
        ChallengeTitleLabel.Text = ChallengeTextLocalizer.GetDisplayTitle(_appState.TodayChallenge.Title);
        ChallengeSubtitleLabel.Text = string.Format(AppStrings.DateStepsFormat, DateHelpers.ToDisplayDate(_appState.TodayChallenge.Date), completedSteps, totalSteps);
        RenderPersonalization();
        ChallengeProgressBar.Progress = completedSteps / (double)totalSteps;
        ChallengeButton.Text = isCompleted
            ? AppStrings.GoToNextDay
            : AppStrings.OpenDailyChallenge;
        ChallengeButton.IsEnabled = true;
    }

    private void RenderPersonalization()
    {
        var profile = ChallengePersonalizer.CreateProfile(_appState.StartSelfAssessment);

        PersonalizationLabel.IsVisible = profile is not null;
        PersonalizationLabel.Text = profile?.DailyReason ?? string.Empty;
    }

    private async void OnOpenChallengeClicked(object? sender, EventArgs e)
    {
        if (_appState.TodayChallenge is null)
        {
            return;
        }

        if (_appState.TodayChallenge.Status == ChallengeStatus.Completed)
        {
            var nextChallenge = _appState.AdvanceToNextDay();
            _appState.TrackChallengeOpened(nextChallenge);
            await Navigation.PushAsync(new ChallengeDetailPage(_appState, nextChallenge.Date));
            return;
        }

        _appState.TrackChallengeOpened(_appState.TodayChallenge);
        await Navigation.PushAsync(new ChallengeDetailPage(_appState, _appState.TodayChallenge.Date));
    }

    private async void OnOpenStatisticsClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new StatisticsPage(_appState));
    }

    private async void OnOpenProfileClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new ProfilePage(_appState));
    }

    private async void OnOpenSettingsClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new SettingsPage(_appState, _backendConnectionService, _localizationService));
    }

    private async Task SyncBackendAndRenderAsync()
    {
        var result = await _appState.SyncBackendAsync();

        if (result.Succeeded && result.Stats is not null)
        {
            Render();
        }
    }

    private async Task TryOpenFinalAssessmentAsync()
    {
        if (_isOpeningFinalAssessment || !_appState.ShouldShowFinalSelfAssessment)
        {
            return;
        }

        _isOpeningFinalAssessment = true;

        await Navigation.PushAsync(new SelfAssessmentPage(
            _appState,
            SelfAssessmentKind.Final,
            async () =>
            {
                if (Navigation.NavigationStack.Count > 1)
                {
                    await Navigation.PopAsync();
                }

                _isOpeningFinalAssessment = false;
                Render();
            }));
    }

    private string ShortName(string name)
    {
        var trimmed = name.Trim();

        return trimmed.Length > 0 ? trimmed : AppStrings.DefaultDisplayName;
    }
}
