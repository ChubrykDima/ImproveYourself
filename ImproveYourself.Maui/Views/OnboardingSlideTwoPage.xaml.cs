using ImproveYourself.Maui.Application;
using ImproveYourself.Maui.Resources.Strings;

namespace ImproveYourself.Maui.Views;

public partial class OnboardingSlideTwoPage : ContentPage
{
    private readonly AppState _appState;
    private readonly Func<Task> _navigateToHomeAsync;
    private bool _ready;

    public OnboardingSlideTwoPage(AppState appState, Func<Task> navigateToHomeAsync)
    {
        InitializeComponent();
        _appState = appState;
        _navigateToHomeAsync = navigateToHomeAsync;

        TitleLabel.Text = AppStrings.Onboarding2_Title;
        BodyLabel.Text = AppStrings.Onboarding2_Body;
        BackButton.Text = AppStrings.Back;
        NextButton.Text = AppStrings.Next;
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

    private async void OnNextClicked(object? sender, EventArgs e)
    {
        if (!TryBeginNavigation())
        {
            return;
        }

        SetNavigationEnabled(false);

        try
        {
            await Navigation.PushAsync(new OnboardingSlideThreePage(_appState, _navigateToHomeAsync));
        }
        finally
        {
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
        NextButton.IsEnabled = enabled;
    }
}
