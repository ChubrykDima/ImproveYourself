using ImproveYourself.Maui.Application;

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
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnNextClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new OnboardingSlideThreePage(_appState, _navigateToHomeAsync));
    }
}
