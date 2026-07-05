using ImproveYourself.Maui.Application;
using ImproveYourself.Maui.Domain;

namespace ImproveYourself.Maui.Views;

public partial class StatisticsPage : ContentPage
{
    private static readonly string[] DayLabels = ["Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс"];

    private readonly AppState _appState;

    public StatisticsPage(AppState appState)
    {
        InitializeComponent();
        _appState = appState;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _appState.RefreshDerivedState();
        Render();
        await _appState.SyncBackendAsync();
        Render();
    }

    private void Render()
    {
        CurrentStreakLabel.Text = _appState.StreakSnapshot.CurrentStreakDays.ToString();
        BestStreakLabel.Text = _appState.StreakSnapshot.BestStreakDays.ToString();

        var stats = _appState.WeeklyStats;

        WeekCompletedLabel.Text = $"Закрыто дней: {stats.TotalCompletedDays}/7";
        WeeklyCompletionRateLabel.Text = $"Общий completion rate: {stats.CompletionRate}%";

        PracticeValueLabel.Text = GetCategoryValue(stats, StepType.Practice).ToString();
        SocialValueLabel.Text = GetCategoryValue(stats, StepType.Social).ToString();

        RenderBackendStats();
        RenderWeeklyBars(stats.Days);
    }

    private void RenderBackendStats()
    {
        var backendStats = _appState.BackendStats;
        var hasMessage = !string.IsNullOrWhiteSpace(_appState.BackendSyncMessage);

        BackendStatsBorder.IsVisible = backendStats is not null || hasMessage;

        if (backendStats is null)
        {
            BackendStatsLabel.Text = hasMessage ? _appState.BackendSyncMessage : string.Empty;
            return;
        }

        BackendStatsLabel.Text =
            $"Сервер: {backendStats.TotalChallengesCompleted} дней, {backendStats.TotalStepsCompleted} шагов. {_appState.BackendSyncMessage}";
    }

    private void RenderWeeklyBars(IReadOnlyList<WeeklyDayStat> days)
    {
        WeeklyBarsContainer.Children.Clear();

        for (var index = 0; index < Math.Min(7, days.Count); index += 1)
        {
            var day = days[index];
            var totalSteps = Math.Max(day.TotalSteps, 1);
            var value = Math.Clamp(day.CompletedSteps, 0, totalSteps);
            var barHeight = Math.Max(value / (double)totalSteps * 96d, 4d);

            var barHost = new Grid
            {
                HeightRequest = 110,
                WidthRequest = 28,
            };

            barHost.Children.Add(new Border
            {
                WidthRequest = 24,
                HeightRequest = barHeight,
                StrokeThickness = 0,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 8 },
                BackgroundColor = day.IsCompleted ? Color.FromArgb("#FF8A3D") : Color.FromArgb("#1C2636"),
                VerticalOptions = LayoutOptions.End,
                HorizontalOptions = LayoutOptions.Center,
            });

            var dayColumn = new VerticalStackLayout
            {
                Spacing = 8,
                HorizontalOptions = LayoutOptions.Center,
            };

            dayColumn.Children.Add(barHost);
            dayColumn.Children.Add(new Label
            {
                Text = DayLabels[index],
                FontFamily = "OpenSansSemibold",
                FontSize = 12,
                TextColor = Color.FromArgb("#9EA8BD"),
                HorizontalTextAlignment = TextAlignment.Center,
            });

            WeeklyBarsContainer.Children.Add(dayColumn);
        }
    }

    private static int GetCategoryValue(WeeklyStats stats, StepType stepType)
    {
        return stats.CategoryBreakdown.TryGetValue(stepType, out var value) ? value : 0;
    }
}
