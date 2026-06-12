using ImproveYourself.Maui.Application;
using ImproveYourself.Maui.Domain;

namespace ImproveYourself.Maui.Views;

public partial class ChallengeDetailPage : ContentPage
{
    private readonly AppState _appState;
    private string _currentDate;
    private DailyChallenge? _challenge;

    public ChallengeDetailPage(AppState appState, string date)
    {
        InitializeComponent();
        _appState = appState;
        _currentDate = date;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _challenge = string.IsNullOrWhiteSpace(_currentDate) ? null : _appState.EnsureChallengeForDate(_currentDate);
        Render();
    }

    private void Render()
    {
        StepsContainer.Children.Clear();

        if (_challenge is null)
        {
            DateLabel.Text = "Дата: --";
            ChallengeTitleLabel.Text = "Челлендж не найден";
            ProgressLabel.Text = "Прогресс: 0/3";
            ChallengeProgressBar.Progress = 0;
            CompletedTextLabel.IsVisible = false;
            NextDayButton.IsVisible = false;
            return;
        }

        var completedSteps = ProgressCalculator.CountCompletedSteps(_challenge.Steps);
        var totalSteps = Math.Max(_challenge.Steps.Count, 1);
        var isCompleted = _challenge.Status == ChallengeStatus.Completed;

        DateLabel.Text = $"Дата: {DateHelpers.ToDisplayDate(_challenge.Date)}";
        ChallengeTitleLabel.Text = _challenge.Title;
        ProgressLabel.Text = $"Прогресс: {completedSteps}/{totalSteps}";
        ChallengeProgressBar.Progress = completedSteps / (double)totalSteps;
        CompletedTextLabel.IsVisible = isCompleted;
        NextDayButton.IsVisible = isCompleted;
        NextDayButton.IsEnabled = isCompleted;

        foreach (var step in _challenge.Steps.OrderBy(step => step.SortOrder))
        {
            StepsContainer.Children.Add(BuildStepCard(step));
        }
    }

    private View BuildStepCard(ChallengeStep step)
    {
        var isCompleted = step.Status == StepStatus.Completed;
        var isInProgress = step.Status == StepStatus.InProgress;

        var strokeColor = isCompleted
            ? Color.FromArgb("#56D89A")
            : isInProgress
                ? Color.FromArgb("#FF8A3D")
                : Color.FromArgb("#2C3648");

        var card = new Border
        {
            Stroke = strokeColor,
            StrokeThickness = 1,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 24 },
            BackgroundColor = Color.FromArgb("#161D29"),
            Padding = 22,
        };

        var stack = new VerticalStackLayout { Spacing = 10 };

        var headerGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
        };

        headerGrid.Add(new Label
        {
            Text = step.Title,
            FontFamily = "OpenSansSemibold",
            FontSize = 18,
            TextColor = Color.FromArgb("#F5F7FB"),
            LineBreakMode = LineBreakMode.WordWrap,
        });

        headerGrid.Add(new Label
        {
            Text = step.Status.ToDisplayTitle(),
            FontFamily = "OpenSansRegular",
            FontSize = 15,
            TextColor = Color.FromArgb("#9EA8BD"),
            HorizontalTextAlignment = TextAlignment.End,
            VerticalTextAlignment = TextAlignment.Start,
        }, 1, 0);

        stack.Children.Add(headerGrid);

        if (!string.IsNullOrWhiteSpace(step.Subtitle))
        {
            stack.Children.Add(new Label
            {
                Text = step.Subtitle,
                FontFamily = "OpenSansRegular",
                FontSize = 15,
                TextColor = Color.FromArgb("#FF8A3D"),
            });
        }

        stack.Children.Add(new Label
        {
            Text = step.Description,
            FontFamily = "OpenSansRegular",
            FontSize = 15,
            TextColor = Color.FromArgb("#F5F7FB"),
            LineBreakMode = LineBreakMode.WordWrap,
        });

        if (step.Type == StepType.Quote && !string.IsNullOrWhiteSpace(step.QuoteText))
        {
            var quoteGrid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(new GridLength(3)),
                    new ColumnDefinition(GridLength.Star),
                },
            };

            quoteGrid.Add(new BoxView
            {
                Color = Color.FromArgb("#FF8A3D"),
                WidthRequest = 3,
            }, 0, 0);

            var quoteStack = new VerticalStackLayout
            {
                Spacing = 6,
                Padding = new Thickness(12, 0, 0, 0),
            };

            quoteStack.Children.Add(new Label
            {
                Text = $"\u201C{step.QuoteText}\u201D",
                FontFamily = "OpenSansRegular",
                FontSize = 15,
                TextColor = Color.FromArgb("#F5F7FB"),
                LineBreakMode = LineBreakMode.WordWrap,
            });

            if (!string.IsNullOrWhiteSpace(step.QuoteAuthor))
            {
                quoteStack.Children.Add(new Label
                {
                    Text = $"\u2014 {step.QuoteAuthor}",
                    FontFamily = "OpenSansRegular",
                    FontSize = 15,
                    TextColor = Color.FromArgb("#9EA8BD"),
                });
            }

            quoteGrid.Add(quoteStack, 1, 0);
            stack.Children.Add(quoteGrid);
        }

        if (!string.IsNullOrWhiteSpace(step.Tip))
        {
            stack.Children.Add(new Label
            {
                Text = $"Совет: {step.Tip}",
                FontFamily = "OpenSansRegular",
                FontSize = 15,
                TextColor = Color.FromArgb("#9EA8BD"),
                LineBreakMode = LineBreakMode.WordWrap,
            });
        }

        var button = new Button
        {
            Text = step.Status.ToActionTitle(),
            Style = (Style)Microsoft.Maui.Controls.Application.Current!.Resources[isCompleted ? "GhostButtonStyle" : "PrimaryButtonStyle"],
            IsEnabled = !isCompleted,
        };

        button.Clicked += async (_, _) => await OnAdvanceStep(step.Type);

        stack.Children.Add(button);
        card.Content = stack;

        return card;
    }

    private Task OnAdvanceStep(StepType stepType)
    {
        if (_challenge is null)
        {
            return Task.CompletedTask;
        }

        _challenge = _appState.AdvanceStep(_challenge.Date, stepType);

        // If the challenge is completed but AdvanceStep didn't auto-advance, trigger it now.
        if (_challenge.Status == ChallengeStatus.Completed)
        {
            _challenge = _appState.AdvanceToNextDay();
        }

        _currentDate = _challenge.Date;
        Render();

        return Task.CompletedTask;
    }

    private void OnOpenNextDayClicked(object? sender, EventArgs e)
    {
        if (_challenge is null || _challenge.Status != ChallengeStatus.Completed)
        {
            return;
        }

        _challenge = _appState.AdvanceToNextDay();
        _currentDate = _challenge.Date;
        Render();
    }
}
