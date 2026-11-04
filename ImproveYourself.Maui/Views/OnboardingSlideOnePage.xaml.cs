using ImproveYourself.Maui.Application;
using ImproveYourself.Maui.Resources.Strings;

namespace ImproveYourself.Maui.Views;

public partial class OnboardingSlideOnePage : ContentPage
{
    private readonly AppState _appState;
    private readonly Func<Task> _navigateToHomeAsync;

    public OnboardingSlideOnePage(AppState appState, Func<Task> navigateToHomeAsync)
    {
        InitializeComponent();
        _appState = appState;
        _navigateToHomeAsync = navigateToHomeAsync;

        TitleLabel.Text = AppStrings.Onboarding1_Title;
        BodyLabel.Text = AppStrings.Onboarding1_Body;
        NextButton.Text = AppStrings.Next;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        NextButton.IsEnabled = false;
        OnboardingNavigationGate.RunWhenIdle(() => NextButton.IsEnabled = true);
    }

    private async void OnNextClicked(object? sender, EventArgs e)
    {
        if (!OnboardingNavigationGate.TryEnter())
        {
            return;
        }

        NextButton.IsEnabled = false;

        try
        {
            await Navigation.PushAsync(new OnboardingSlideTwoPage(_appState, _navigateToHomeAsync));
        }
        finally
        {
            OnboardingNavigationGate.Exit();
        }
    }
}
