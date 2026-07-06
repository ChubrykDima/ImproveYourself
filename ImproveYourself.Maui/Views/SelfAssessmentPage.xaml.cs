using ImproveYourself.Maui.Application;
using ImproveYourself.Maui.Domain;
using ImproveYourself.Maui.Resources.Strings;

namespace ImproveYourself.Maui.Views;

public partial class SelfAssessmentPage : ContentPage
{
    private readonly AppState _appState;
    private readonly SelfAssessmentKind _kind;
    private readonly Func<Task> _onCompletedAsync;
    private readonly bool _promptNotificationsOnCompletion;
    private readonly List<int> _scores;
    private bool _isSubmitting;

    public SelfAssessmentPage(
        AppState appState,
        SelfAssessmentKind kind,
        Func<Task> onCompletedAsync,
        bool promptNotificationsOnCompletion = false)
    {
        InitializeComponent();
        _appState = appState;
        _kind = kind;
        _onCompletedAsync = onCompletedAsync;
        _promptNotificationsOnCompletion = promptNotificationsOnCompletion;
        _scores = Enumerable.Repeat(5, SelfAssessmentSurvey.Questions.Count).ToList();

        Title = AppStrings.SelfAssessment_Title;
        RenderHeader();
        RenderQuestions();
    }

    private void RenderHeader()
    {
        if (_kind == SelfAssessmentKind.Start)
        {
            ProgressDots.IsVisible = true;
            EyebrowLabel.Text = AppStrings.BeforeStart;
            TitleLabel.Text = AppStrings.QuickStateCheck;
            DescriptionLabel.Text = AppStrings.SelfAssessmentStartDesc;
            SubmitButton.Text = AppStrings.SaveAndStart;
            return;
        }

        ProgressDots.IsVisible = false;
        EyebrowLabel.Text = AppStrings.FinalAssessment;
        TitleLabel.Text = AppStrings.WhatChangedIn30Days;
        DescriptionLabel.Text = AppStrings.SelfAssessmentFinalDesc;
        SubmitButton.Text = AppStrings.ShowResult;
    }

    private void RenderQuestions()
    {
        QuestionsContainer.Children.Clear();

        for (var index = 0; index < SelfAssessmentSurvey.Questions.Count; index += 1)
        {
            var questionIndex = index;
            var question = SelfAssessmentSurvey.Questions[index];
            var valueLabel = new Label
            {
                Text = _scores[questionIndex].ToString(),
                FontFamily = "OpenSansSemibold",
                FontSize = 22,
                TextColor = Color.FromArgb("#F5F7FB"),
                HorizontalTextAlignment = TextAlignment.End,
                VerticalTextAlignment = TextAlignment.Center,
            };

            var slider = new Slider
            {
                Minimum = 1,
                Maximum = 10,
                Value = _scores[questionIndex],
                MinimumTrackColor = Color.FromArgb("#FF8A3D"),
                MaximumTrackColor = Color.FromArgb("#2C3648"),
                ThumbColor = Color.FromArgb("#F5F7FB"),
            };

            slider.ValueChanged += (_, args) =>
            {
                var score = (int)Math.Round(args.NewValue, MidpointRounding.AwayFromZero);
                _scores[questionIndex] = score;
                valueLabel.Text = score.ToString();
            };

            var header = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto),
                },
                ColumnSpacing = 16,
            };

            header.Add(new Label
            {
                Text = question.Text,
                FontFamily = "OpenSansRegular",
                FontSize = 15,
                TextColor = Color.FromArgb("#F5F7FB"),
                LineBreakMode = LineBreakMode.WordWrap,
            }, 0, 0);
            header.Add(valueLabel, 1, 0);

            var scale = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Star),
                },
            };

            scale.Add(new Label
            {
                Text = "1",
                FontFamily = "OpenSansRegular",
                FontSize = 13,
                TextColor = Color.FromArgb("#9EA8BD"),
            }, 0, 0);
            scale.Add(new Label
            {
                Text = "10",
                FontFamily = "OpenSansRegular",
                FontSize = 13,
                TextColor = Color.FromArgb("#9EA8BD"),
                HorizontalTextAlignment = TextAlignment.End,
            }, 1, 0);

            var stack = new VerticalStackLayout { Spacing = 8 };
            stack.Children.Add(header);
            stack.Children.Add(slider);
            stack.Children.Add(scale);

            QuestionsContainer.Children.Add(new Border
            {
                Stroke = Color.FromArgb("#2C3648"),
                StrokeThickness = 1,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 24 },
                BackgroundColor = Color.FromArgb("#161D29"),
                Padding = 18,
                Content = stack,
            });
        }
    }

    private async void OnSubmitClicked(object? sender, EventArgs e)
    {
        if (_isSubmitting)
        {
            return;
        }

        _isSubmitting = true;
        SubmitButton.IsEnabled = false;

        var snapshot = SelfAssessmentSurvey.Create(_kind, _scores);
        _appState.SaveSelfAssessment(snapshot);

        if (_kind == SelfAssessmentKind.Final && _appState.StartSelfAssessment is not null)
        {
            await ShowFinalComparisonAsync(_appState.StartSelfAssessment, snapshot);
        }

        if (_promptNotificationsOnCompletion)
        {
            await PromptNotificationsAsync();
        }

        await _onCompletedAsync();
    }

    private Task ShowFinalComparisonAsync(SelfAssessmentSnapshot start, SelfAssessmentSnapshot final)
    {
        var delta = final.AverageScore - start.AverageScore;
        var direction = delta >= 0 ? "+" : string.Empty;
        var message = string.Format(AppStrings.FinalComparisonFormat, start.AverageScore, final.AverageScore, direction, delta);

        return DisplayAlertAsync(AppStrings.YourResult, message, AppStrings.Done);
    }

    private async Task PromptNotificationsAsync()
    {
        var choice = await DisplayActionSheetAsync(AppStrings.Notifications_Title, AppStrings.Later, null, AppStrings.Enable);

        if (choice != AppStrings.Enable)
        {
            return;
        }

        var enabled = await _appState.SetNotificationsEnabledAsync(true);

        if (!enabled)
        {
            await DisplayAlertAsync(AppStrings.PermissionNotGranted, AppStrings.EnableNotificationsLater, AppStrings.OK);
        }
    }
}
