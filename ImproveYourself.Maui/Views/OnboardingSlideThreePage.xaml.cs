using ImproveYourself.Maui.Application;
using ImproveYourself.Maui.Domain;
using ImproveYourself.Maui.Resources.Strings;

namespace ImproveYourself.Maui.Views;

public partial class OnboardingSlideThreePage : ContentPage
{
    private readonly AppState _appState;
    private readonly Func<Task> _navigateToHomeAsync;

    public OnboardingSlideThreePage(AppState appState, Func<Task> navigateToHomeAsync)
    {
        InitializeComponent();
        _appState = appState;
        _navigateToHomeAsync = navigateToHomeAsync;

        TitleLabel.Text = AppStrings.Onboarding3_Title;
        BodyLabel.Text = AppStrings.Onboarding3_Body;
        HowToAddressLabel.Text = AppStrings.HowToAddress;
        DisplayNameEntry.Placeholder = AppStrings.EnterNamePlaceholder;
        DisplayNameEntry.Text = _appState.DisplayName;
        BackButton.Text = AppStrings.Back;
        FinishButton.Text = AppStrings.GoToSelfAssessment;
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnFinishClicked(object? sender, EventArgs e)
    {
        var displayName = DisplayNameEntry.Text ?? string.Empty;
        _appState.UpdateDisplayName(displayName);

        await Navigation.PushAsync(new SelfAssessmentPage(
            _appState,
            SelfAssessmentKind.Start,
            async () =>
            {
                await _appState.CompleteOnboardingAsync(displayName);
                await _navigateToHomeAsync();
            },
            promptNotificationsOnCompletion: true));
    }
}
