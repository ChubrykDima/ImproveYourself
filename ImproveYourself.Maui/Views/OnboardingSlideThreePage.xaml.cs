using ImproveYourself.Maui.Application;
using ImproveYourself.Maui.Domain;
using ImproveYourself.Maui.Resources.Strings;

namespace ImproveYourself.Maui.Views;

public partial class OnboardingSlideThreePage : ContentPage
{
    private readonly AppState _appState;
    private readonly Func<Task> _navigateToHomeAsync;
    private bool _isNavigating;

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
        FinishButton.Text = AppStrings.Next;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _isNavigating = false;
        FinishButton.IsEnabled = true;
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        if (_isNavigating)
        {
            return;
        }

        _isNavigating = true;
        await Navigation.PopAsync();
    }

    private async void OnFinishClicked(object? sender, EventArgs e)
    {
        if (_isNavigating)
        {
            return;
        }

        var validation = AccountInput.ValidateDisplayName(DisplayNameEntry.Text);

        if (validation is not null)
        {
            await DisplayAlertAsync(AppStrings.Profile, validation, AppStrings.OK);
            return;
        }

        _isNavigating = true;
        FinishButton.IsEnabled = false;
        var displayName = DisplayNameEntry.Text?.Trim() ?? string.Empty;
        _appState.UpdateDisplayName(displayName);
        await Navigation.PushAsync(new OnboardingAccountPage(_appState, _navigateToHomeAsync, displayName));
    }
}
