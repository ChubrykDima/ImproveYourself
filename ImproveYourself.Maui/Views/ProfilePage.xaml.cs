using ImproveYourself.Maui.Application;
using ImproveYourself.Maui.Domain;

namespace ImproveYourself.Maui.Views;

public partial class ProfilePage : ContentPage
{
    private readonly AppState _appState;

    public ProfilePage(AppState appState)
    {
        InitializeComponent();
        _appState = appState;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _appState.RefreshDerivedState();
        Render();
    }

    private void Render()
    {
        var name = string.IsNullOrWhiteSpace(_appState.DisplayName) ? "Друг" : _appState.DisplayName.Trim();

        DisplayNameLabel.Text = name;
        AvatarLabel.Text = name[..1].ToUpperInvariant();

        CurrentStreakMetricLabel.Text = _appState.StreakSnapshot.CurrentStreakDays.ToString();
        TotalCompletedMetricLabel.Text = _appState.CompletedDates.Count.ToString();
        CompletionRateMetricLabel.Text = $"{ProgressCalculator.CalculateRollingCompletionRate(_appState.CompletedDates)}%";

        RenderCalendar();
    }

    private void RenderCalendar()
    {
        CalendarGrid.Children.Clear();
        CalendarGrid.RowDefinitions.Clear();

        var today = DateTime.Today;
        var todayIsoDate = DateHelpers.ToIsoDate(today);
        var calendarDays = ProgressCalculator.ListCalendarDaysForMonth(today);
        var placeholders = GetLeadingPlaceholders(today);

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
                StrokeThickness = 1,
                Stroke = isToday ? Color.FromArgb("#F5F7FB") : Colors.Transparent,
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

            if (isCompleted)
            {
                cell.Stroke = Color.FromArgb("#FF8A3D");
            }

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
