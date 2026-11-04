using ImproveYourself.Maui.Application;
using ImproveYourself.Maui.Resources.Strings;

namespace ImproveYourself.Maui.Views;

public partial class OnboardingSlideTwoPage : ContentPage
{
    private readonly AppState _appState;
    private readonly Func<Task> _navigateToHomeAsync;

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
        SetNavigationEnabled(false);
        OnboardingNavigationGate.RunWhenIdle(() => SetNavigationEnabled(true));
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        if (!OnboardingNavigationGate.TryEnter())
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
            OnboardingNavigationGate.Exit();
        }
    }

    private async void OnNextClicked(object? sender, EventArgs e)
    {
        if (!OnboardingNavigationGate.TryEnter())
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
            OnboardingNavigationGate.Exit();
        }
    }

    private void SetNavigationEnabled(bool enabled)
    {
        BackButton.IsEnabled = enabled;
        NextButton.IsEnabled = enabled;
    }
}
