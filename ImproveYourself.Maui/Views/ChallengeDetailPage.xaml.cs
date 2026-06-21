using ImproveYourself.Maui.Application;
using ImproveYourself.Maui.Domain;

namespace ImproveYourself.Maui.Views;

public partial class ChallengeDetailPage : ContentPage
{
    private readonly AppState _appState;
    private string _currentDate;
    private DailyChallenge? _challenge;
    private bool _isQuoteNoteVisible;

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
            ProgressLabel.Text = "Прогресс: 0/2";
            ChallengeProgressBar.Progress = 0;
            CompletedTextLabel.IsVisible = false;
            NextDayButton.IsVisible = false;
            QuoteNoteBorder.IsVisible = false;
            QuoteSectionBorder.IsVisible = false;
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
        RenderQuoteSection(_challenge);

        foreach (var step in _challenge.Steps
                     .Where(step => step.Type != StepType.Quote)
                     .OrderBy(step => step.SortOrder))
        {
            StepsContainer.Children.Add(BuildStepCard(step));
        }
    }

    private void RenderQuoteSection(DailyChallenge challenge)
    {
        var hasQuote = !string.IsNullOrWhiteSpace(challenge.QuoteText);

        QuoteSectionBorder.IsVisible = hasQuote;

        if (!hasQuote)
        {
            _isQuoteNoteVisible = false;
            QuoteNoteBorder.IsVisible = false;
            QuoteNoteLabel.Text = string.Empty;
            QuoteTextLabel.Text = string.Empty;
            QuoteAuthorLabel.Text = string.Empty;
            QuoteAuthorLabel.IsVisible = false;
            QuoteHintLabel.IsVisible = false;
            QuoteSectionBorder.Stroke = (Color)Microsoft.Maui.Controls.Application.Current!.Resources["ColorBorder"];
            return;
        }

        QuoteTextLabel.Text = $"\u201C{challenge.QuoteText}\u201D";
        QuoteAuthorLabel.IsVisible = !string.IsNullOrWhiteSpace(challenge.QuoteAuthor);
        QuoteAuthorLabel.Text = QuoteAuthorLabel.IsVisible
            ? $"\u2014 {challenge.QuoteAuthor}"
            : string.Empty;

        var hasQuoteNote = !string.IsNullOrWhiteSpace(challenge.QuoteNote);
        QuoteHintLabel.IsVisible = hasQuoteNote;
        QuoteHintLabel.Text = _isQuoteNoteVisible
            ? "Нажми, чтобы скрыть пояснение"
            : "Нажми, чтобы открыть пояснение";
        QuoteSectionBorder.Stroke = hasQuoteNote
            ? Color.FromArgb("#2C3648")
            : (Color)Microsoft.Maui.Controls.Application.Current!.Resources["ColorBorder"];
        UpdateQuoteNoteVisibility(challenge, hasQuoteNote);
    }

    private void UpdateQuoteNoteVisibility(DailyChallenge challenge, bool hasQuoteNote)
    {
        if (!hasQuoteNote)
        {
            _isQuoteNoteVisible = false;
            QuoteNoteBorder.IsVisible = false;
            QuoteNoteLabel.Text = string.Empty;
            return;
        }

        QuoteNoteBorder.IsVisible = _isQuoteNoteVisible;
        QuoteNoteLabel.Text = _isQuoteNoteVisible
            ? challenge.QuoteNote
            : string.Empty;
    }

    private void OnQuoteTapped(object? sender, TappedEventArgs e)
    {
        if (_challenge is null || string.IsNullOrWhiteSpace(_challenge.QuoteNote))
        {
            return;
        }

        _isQuoteNoteVisible = !_isQuoteNoteVisible;
        RenderQuoteSection(_challenge);
    }

    private void OnScreenTapped(object? sender, TappedEventArgs e)
    {
        if (_challenge is null || !_isQuoteNoteVisible)
        {
            return;
        }

        var tapOnQuote = e.GetPosition(QuoteSectionBorder) is Point point
            && point.X >= 0
            && point.Y >= 0
            && point.X <= QuoteSectionBorder.Width
            && point.Y <= QuoteSectionBorder.Height;

        if (tapOnQuote)
        {
            return;
        }

        _isQuoteNoteVisible = false;
        RenderQuoteSection(_challenge);
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
