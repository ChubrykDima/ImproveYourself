using ImproveYourself.Maui.Application;
using ImproveYourself.Maui.Domain;
using ImproveYourself.Maui.Resources.Strings;

namespace ImproveYourself.Maui.Views;

public partial class ProfilePage : ContentPage
{
    private readonly AppState _appState;
    private DateTime _visibleMonth = DateTime.Today;

    public ProfilePage(AppState appState)
    {
        InitializeComponent();
        _appState = appState;

        Title = AppStrings.ProfileCalendar_Title;
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
        CurrentStreakMetricTitleLabel.Text = AppStrings.CurrentStreak;
        AllChallengesLabel.Text = AppStrings.AllChallenges;
        CompletionRateTitleLabel.Text = AppStrings.CompletionRateLabel;
        SelfAssessmentTitleLabel.Text = AppStrings.SelfAssessmentLabel;
        MonthActivityTitleLabel.Text = AppStrings.MonthActivity;
        DayMonLabel.Text = AppStrings.DayMon;
        DayTueLabel.Text = AppStrings.DayTue;
        DayWedLabel.Text = AppStrings.DayWed;
        DayThuLabel.Text = AppStrings.DayThu;
        DayFriLabel.Text = AppStrings.DayFri;
        DaySatLabel.Text = AppStrings.DaySat;
        DaySunLabel.Text = AppStrings.DaySun;
    }

    private void Render()
    {
        var name = string.IsNullOrWhiteSpace(_appState.DisplayName)
            ? AppStrings.DefaultDisplayName
            : _appState.DisplayName.Trim();

        DisplayNameLabel.Text = name;
        AvatarLabel.Text = AccountInput.FirstGrapheme(name).ToUpperInvariant();

        CurrentStreakMetricLabel.Text = _appState.StreakSnapshot.CurrentStreakDays.ToString();
        TotalCompletedMetricLabel.Text = ProgressCalculator.CountDistinctDates(_appState.CompletedDates).ToString();
        CompletionRateMetricLabel.Text = $"{_appState.MonthlyProgress.Percent}%";

        RenderSelfAssessmentSummary();
        RenderCalendar();
    }

    private void RenderSelfAssessmentSummary()
    {
        var start = _appState.StartSelfAssessment;
        var final = _appState.FinalSelfAssessment;

        SelfAssessmentSummaryBorder.IsVisible = start is not null;

        if (start is null)
        {
            SelfAssessmentSummaryLabel.Text = string.Empty;
            SelfAssessmentDeltaLabel.Text = string.Empty;
            SelfAssessmentDeltaLabel.IsVisible = false;
            return;
        }

        if (final is null)
        {
            SelfAssessmentSummaryLabel.Text = string.Format(AppStrings.StartCutFormat, start.AverageScore);
            SelfAssessmentDeltaLabel.Text = string.Empty;
            SelfAssessmentDeltaLabel.IsVisible = false;
            return;
        }

        var delta = final.AverageScore - start.AverageScore;
        var direction = delta >= 0 ? "+" : string.Empty;

        SelfAssessmentSummaryLabel.Text = string.Format(AppStrings.StartFinishFormat, start.AverageScore, final.AverageScore);
        SelfAssessmentDeltaLabel.Text = string.Format(AppStrings.ShiftFormat, direction, delta);
        SelfAssessmentDeltaLabel.IsVisible = true;
    }

    private void OnPreviousMonthClicked(object? sender, EventArgs e)
    {
        _visibleMonth = _visibleMonth.AddMonths(-1);
        RenderCalendar();
    }

    private void OnNextMonthClicked(object? sender, EventArgs e)
    {
        var next = _visibleMonth.AddMonths(1);
        var current = DateTime.Today;

        if (next.Year > current.Year || (next.Year == current.Year && next.Month > current.Month))
        {
            return;
        }

        _visibleMonth = next;
        RenderCalendar();
    }

    private async void OnCalendarDayTapped(string isoDate)
    {
        var completed = _appState.CompletedProgramDaysOn(isoDate);
        var title = DateHelpers.ToDisplayDate(isoDate);
        var message = completed.Count == 0
            ? string.Format(AppStrings.CalendarDayEmptyFormat, title)
            : string.Format(AppStrings.CalendarDayCompletedFormat, title) + "\n" + string.Join("\n", completed);

        await DisplayAlertAsync(title, message, AppStrings.OK);
    }

    private void RenderCalendar()
    {
        CalendarGrid.Children.Clear();
        CalendarGrid.RowDefinitions.Clear();

        var today = DateTime.Today;
        var todayIsoDate = DateHelpers.ToIsoDate(today);
        var month = new DateTime(_visibleMonth.Year, _visibleMonth.Month, 1);
        CalendarMonthLabel.Text = month.ToString("MMMM yyyy", System.Globalization.CultureInfo.CurrentCulture);
        NextMonthButton.IsEnabled = month.Year < today.Year || (month.Year == today.Year && month.Month < today.Month);
        var calendarDays = ProgressCalculator.ListCalendarDaysForMonth(month);
        var placeholders = GetLeadingPlaceholders(month);

        var cells = new List<string>();
        cells.AddRange(Enumerable.Repeat(string.Empty, placeholders));
        cells.AddRange(calendarDays);

        while (cells.Count % 7 != 0)
        {
            cells.Add(string.Empty);
        }

        var rows = cells.Count / 7;

        for (var row = 0; row < rows; row += 1)
        {
            CalendarGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        }

        var completedDates = new HashSet<string>(_appState.CompletedDates, StringComparer.Ordinal);

        for (var index = 0; index < cells.Count; index += 1)
        {
            var isoDate = cells[index];
            var row = index / 7;
            var column = index % 7;

            if (string.IsNullOrWhiteSpace(isoDate))
            {
                CalendarGrid.Add(new BoxView
                {
                    WidthRequest = 38,
                    HeightRequest = 38,
                    Opacity = 0,
                }, column, row);

                continue;
            }

            var isCompleted = completedDates.Contains(isoDate);
            var isToday = isoDate == todayIsoDate;
            var dayValue = int.Parse(isoDate[^2..]);

            var cell = new Border
            {
                WidthRequest = 38,
                HeightRequest = 38,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
                StrokeThickness = isToday ? 2 : 1,
                Stroke = isToday
                    ? Color.FromArgb("#F5F7FB")
                    : isCompleted ? Color.FromArgb("#FF8A3D") : Colors.Transparent,
                BackgroundColor = isCompleted ? Color.FromRgba(255, 138, 61, 64) : Color.FromArgb("#101520"),
                Content = new Label
                {
                    Text = dayValue.ToString(),
                    FontFamily = "OpenSansRegular",
                    FontSize = 14,
                    TextColor = isCompleted ? Color.FromArgb("#F5F7FB") : Color.FromArgb("#9EA8BD"),
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                },
            };
            var tappedDate = isoDate;
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => OnCalendarDayTapped(tappedDate);
            cell.GestureRecognizers.Add(tap);

            CalendarGrid.Add(cell, column, row);
        }
    }

    private static int GetLeadingPlaceholders(DateTime date)
    {
        var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
        var day = (int)firstDayOfMonth.DayOfWeek;

        return (day + 6) % 7;
    }
}
