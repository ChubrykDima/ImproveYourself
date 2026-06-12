using ImproveYourself.Maui.Application;

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
    }

    private async void OnNextClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new OnboardingSlideTwoPage(_appState, _navigateToHomeAsync));
    }
}
