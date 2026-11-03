using System.Text.Json;
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
    private readonly HashSet<int> _answeredQuestions = [];
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
        RestoreDraft();
        RenderHeader();
        RenderQuestions();
        UpdateSubmitState();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        BackButton.Text = AppStrings.Back;
        BackButton.IsVisible = Navigation.NavigationStack.Count > 1;
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        if (Navigation.NavigationStack.Count > 1)
        {
            await Navigation.PopAsync();
        }
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
            var answered = _answeredQuestions.Contains(questionIndex);
            var valueLabel = new Label
            {
                Text = answered ? _scores[questionIndex].ToString() : "—",
                FontFamily = "OpenSansSemibold",
                FontSize = 22,
                TextColor = Color.FromArgb(answered ? "#F5F7FB" : "#9EA8BD"),
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

            void MarkQuestionAnswered()
            {
                _answeredQuestions.Add(questionIndex);
                valueLabel.Text = _scores[questionIndex].ToString();
                valueLabel.TextColor = Color.FromArgb("#F5F7FB");
                UpdateSubmitState();
                PersistDraft();
            }

            slider.DragStarted += (_, _) => MarkQuestionAnswered();
            slider.ValueChanged += (_, args) =>
            {
                var score = (int)Math.Round(args.NewValue, MidpointRounding.AwayFromZero);
                _scores[questionIndex] = score;
                MarkQuestionAnswered();
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
        if (_isSubmitting || _answeredQuestions.Count != SelfAssessmentSurvey.Questions.Count)
        {
            return;
        }

        _isSubmitting = true;
        SubmitButton.IsEnabled = false;
        var finished = false;

        try
        {
            var snapshot = SelfAssessmentSurvey.Create(_kind, _scores);
            _appState.SaveSelfAssessment(snapshot);
            _appState.ClearAssessmentDraft(_kind);

            if (_kind == SelfAssessmentKind.Final && _appState.StartSelfAssessment is not null)
            {
                ShowFinalResult(_appState.StartSelfAssessment, snapshot);
                finished = true;
                return;
            }

            if (_promptNotificationsOnCompletion)
            {
                await PromptNotificationsAsync();
            }

            await _onCompletedAsync();
            finished = true;
        }
        finally
        {
            if (!finished)
            {
                _isSubmitting = false;
                UpdateSubmitState();
            }
        }
    }

    private async void OnResultContinueClicked(object? sender, EventArgs e)
    {
        ResultContinueButton.IsEnabled = false;
        await _onCompletedAsync();
    }

    private void ShowFinalResult(SelfAssessmentSnapshot start, SelfAssessmentSnapshot final)
    {
        var delta = final.AverageScore - start.AverageScore;
        var direction = delta >= 0 ? "+" : string.Empty;
        var summary = string.Format(AppStrings.FinalComparisonFormat, start.AverageScore, final.AverageScore, direction, delta);
        var details = SelfAssessmentSurvey.Compare(start, final)
            .Select(change =>
            {
                var changeDirection = change.Delta >= 0 ? "+" : string.Empty;
                return string.Format(
                    AppStrings.AssessmentQuestionChangeFormat,
                    change.Question.Text,
                    change.StartScore,
                    change.FinalScore,
                    changeDirection,
                    change.Delta);
            });

        QuestionsContainer.IsVisible = false;
        SubmitButton.IsVisible = false;
        AssessmentValidationLabel.IsVisible = false;
        ResultTitleLabel.Text = AppStrings.ProgramCongratulationsTitle;
        ResultBodyLabel.Text = $"{AppStrings.ProgramCongratulationsBody}\n\n{summary}\n\n{string.Join("\n", details)}";
        ResultContinueButton.Text = AppStrings.Done;
        ResultBorder.IsVisible = true;
    }

    private void PersistDraft()
    {
        var draft = JsonSerializer.Serialize(new AssessmentDraft(
            _scores.ToArray(),
            _answeredQuestions.OrderBy(index => index).ToArray()));
        _appState.WriteAssessmentDraft(_kind, draft);
    }

    private void RestoreDraft()
    {
        var json = _appState.ReadAssessmentDraft(_kind);

        if (string.IsNullOrWhiteSpace(json))
        {
            return;
        }

        try
        {
            var draft = JsonSerializer.Deserialize<AssessmentDraft>(json);

            if (draft?.Scores is null || draft.Scores.Length != _scores.Count)
            {
                return;
            }

            for (var index = 0; index < _scores.Count; index += 1)
            {
                _scores[index] = Math.Clamp(draft.Scores[index], 1, 10);
            }

            foreach (var index in draft.Answered ?? [])
            {
                if (index >= 0 && index < _scores.Count)
                {
                    _answeredQuestions.Add(index);
                }
            }
        }
        catch
        {
            // A broken draft should not block a fresh assessment.
        }
    }

    private sealed record AssessmentDraft(int[] Scores, int[] Answered);

    private void UpdateSubmitState()
    {
        var remaining = SelfAssessmentSurvey.Questions.Count - _answeredQuestions.Count;
        SubmitButton.IsEnabled = !_isSubmitting && remaining == 0;
        AssessmentValidationLabel.IsVisible = remaining > 0;
        AssessmentValidationLabel.Text = remaining > 0
            ? string.Format(AppStrings.AssessmentQuestionsRemainingFormat, remaining)
            : string.Empty;
    }

    private async Task PromptNotificationsAsync()
    {
        var enable = await DisplayAlertAsync(
            AppStrings.Notifications_Title,
            AppStrings.RemindersPrompt,
            AppStrings.Enable,
            AppStrings.Later);

        if (!enable)
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
