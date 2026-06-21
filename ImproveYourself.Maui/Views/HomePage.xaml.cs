using ImproveYourself.Maui.Application;
using ImproveYourself.Maui.Domain;

namespace ImproveYourself.Maui.Views;

public partial class HomePage : ContentPage
{
    private readonly AppState _appState;
    private bool _isOpeningFinalAssessment;

    public HomePage(AppState appState)
    {
        InitializeComponent();
        _appState = appState;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _appState.RefreshDerivedState();
        Render();
        await TryOpenFinalAssessmentAsync();
    }

    private void Render()
    {
        var name = ShortName(_appState.DisplayName);
        GreetingNameLabel.Text = name;
        AvatarInitialLabel.Text = name[..1].ToUpperInvariant();

        StreakValueLabel.Text = _appState.StreakSnapshot.CurrentStreakDays.ToString();
        MonthPercentLabel.Text = $"{_appState.MonthlyProgress.Percent}%";
        MonthHintLabel.Text = $"{_appState.MonthlyProgress.CompletedDays}/{_appState.MonthlyProgress.TargetDays} дней";
        RemainingDaysLabel.Text = $"Осталось дней до цели: {_appState.MonthlyProgress.RemainingDays}";

        if (_appState.TodayChallenge is null)
        {
            ChallengeLeadLabel.Text = "Главный фокус дня";
            ChallengeTitleLabel.Text = "Подготавливаем сегодняшний вызов...";
            ChallengeSubtitleLabel.Text = "Попробуй открыть экран через секунду.";
            ChallengeProgressBar.Progress = 0;
            ChallengeButton.Text = "Подождите";
            ChallengeButton.IsEnabled = false;
            return;
        }

        var completedSteps = ProgressCalculator.CountCompletedSteps(_appState.TodayChallenge.Steps);
        var totalSteps = Math.Max(_appState.TodayChallenge.Steps.Count, 1);
        var isCompleted = _appState.TodayChallenge.Status == ChallengeStatus.Completed;
        var isToday = _appState.TodayChallenge.Date == DateHelpers.ToIsoDate(DateTime.Now);

        ChallengeLeadLabel.Text = isToday
            ? "Главный фокус дня"
            : $"Фокус на {DateHelpers.ToDisplayDate(_appState.TodayChallenge.Date)}";
        ChallengeTitleLabel.Text = _appState.TodayChallenge.Title;
        ChallengeSubtitleLabel.Text = $"Дата: {DateHelpers.ToDisplayDate(_appState.TodayChallenge.Date)} · Шагов: {completedSteps}/{totalSteps}";
        ChallengeProgressBar.Progress = completedSteps / (double)totalSteps;
        ChallengeButton.Text = isCompleted
            ? "Перейти к следующему дню"
            : "Открыть ежедневный вызов";
        ChallengeButton.IsEnabled = true;
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
            await Navigation.PushAsync(new ChallengeDetailPage(_appState, nextChallenge.Date));
            return;
        }

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
        await Navigation.PushAsync(new SettingsPage(_appState));
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

    private static string ShortName(string name)
    {
        var trimmed = name.Trim();

        return trimmed.Length > 0 ? trimmed : "Друг";
    }
}
