using ImproveYourself.Maui.Application;
using ImproveYourself.Maui.Domain;
using ImproveYourself.Maui.Resources.Strings;

namespace ImproveYourself.Maui.Views;

public partial class OnboardingSlideThreePage : ContentPage
{
    private readonly AppState _appState;
    private readonly Func<Task> _navigateToHomeAsync;
    private bool _isFinishing;
    private bool _ready;

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
        ArmWhenTransitionEnds();
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        if (!TryBeginNavigation())
        {
            return;
        }

        SetNavigationEnabled(false);

        try
        {
            await Navigation.PopAsync();
        }
        finally
        {
            await OnboardingNavigationGate.ExitAfterTransitionAsync();
        }
    }

    private async void OnFinishClicked(object? sender, EventArgs e)
    {
        if (!_ready || _isFinishing)
        {
            return;
        }

        var validation = AccountInput.ValidateDisplayName(DisplayNameEntry.Text);

        if (validation is not null)
        {
            _isFinishing = true;
            FinishButton.IsEnabled = false;

            try
            {
                await DisplayAlertAsync(AppStrings.Profile, validation, AppStrings.OK);
            }
            finally
            {
                _isFinishing = false;
                OnboardingNavigationGate.RunWhenIdle(() => FinishButton.IsEnabled = true);
            }

            return;
        }

        if (!OnboardingNavigationGate.TryEnter())
        {
            return;
        }

        _ready = false;
        _isFinishing = true;
        SetNavigationEnabled(false);
        var displayName = DisplayNameEntry.Text?.Trim() ?? string.Empty;
        _appState.UpdateDisplayName(displayName);

        try
        {
            await Navigation.PushAsync(new OnboardingAccountPage(_appState, _navigateToHomeAsync, displayName));
        }
        finally
        {
            _isFinishing = false;
            await OnboardingNavigationGate.ExitAfterTransitionAsync();
        }
    }

    private bool TryBeginNavigation()
    {
        if (!_ready || !OnboardingNavigationGate.TryEnter())
        {
            return false;
        }

        _ready = false;
        return true;
    }

    private void ArmWhenTransitionEnds()
    {
        _ready = false;
        SetNavigationEnabled(false);
        OnboardingNavigationGate.RunWhenIdle(() =>
        {
            SetNavigationEnabled(true);
            Dispatcher.Dispatch(() => _ready = true);
        });
    }

    private void SetNavigationEnabled(bool enabled)
    {
        BackButton.IsEnabled = enabled;
        FinishButton.IsEnabled = enabled;
    }
}
