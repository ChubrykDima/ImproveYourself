using ImproveYourself.Maui.Application;
using ImproveYourself.Maui.Domain;
using ImproveYourself.Maui.Resources.Strings;

namespace ImproveYourself.Maui.Views;

public partial class StatisticsPage : ContentPage
{
    private readonly AppState _appState;

    public StatisticsPage(AppState appState)
    {
        InitializeComponent();
        _appState = appState;

        Title = AppStrings.Statistics_Title;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _appState.RefreshDerivedState();
        RenderStaticLabels();
        Render();
    }

    private void RenderStaticLabels()
    {
        CurrentStreakTitleLabel.Text = AppStrings.CurrentStreak;
        DaysLabel1.Text = AppStrings.Days;
        BestStreakTitleLabel.Text = AppStrings.BestStreak;
        DaysLabel2.Text = AppStrings.Days;
        WeekCompletionTitleLabel.Text = AppStrings.WeekCompletion;
        WeeklyActivityTitleLabel.Text = AppStrings.WeeklyActivity;
        TaskCategoriesTitleLabel.Text = AppStrings.TaskCategories;
        MorningPracticeLabel.Text = AppStrings.MorningPractice;
        DailyChallengeLabel.Text = AppStrings.DailyChallengeName;
    }

    private void Render()
    {
        CurrentStreakLabel.Text = _appState.StreakSnapshot.CurrentStreakDays.ToString();
        BestStreakLabel.Text = _appState.StreakSnapshot.BestStreakDays.ToString();

        var stats = _appState.WeeklyStats;

        WeekCompletedLabel.Text = string.Format(AppStrings.DaysClosedFormat, stats.TotalCompletedDays);
        WeeklyCompletionRateLabel.Text = string.Format(AppStrings.CompletionRateFormat, stats.CompletionRate);

        PracticeValueLabel.Text = GetCategoryValue(stats, StepType.Practice).ToString();
        SocialValueLabel.Text = GetCategoryValue(stats, StepType.Social).ToString();

        RenderWeeklyBars(stats.Days);
    }

    private void RenderWeeklyBars(IReadOnlyList<WeeklyDayStat> days)
    {
        WeeklyBarsContainer.Children.Clear();

        string[] dayLabels =
        [
            AppStrings.DayMon, AppStrings.DayTue, AppStrings.DayWed, AppStrings.DayThu,
            AppStrings.DayFri, AppStrings.DaySat, AppStrings.DaySun,
        ];

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
                Text = dayLabels[index],
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
