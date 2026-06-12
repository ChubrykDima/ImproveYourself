using ImproveYourself.Maui.Application;

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

        DisplayNameEntry.Text = _appState.DisplayName;
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnFinishClicked(object? sender, EventArgs e)
    {
        await _appState.CompleteOnboardingAsync(DisplayNameEntry.Text ?? string.Empty);

        var choice = await DisplayActionSheetAsync("Напоминания", "Позже", null, "Включить");

        if (choice == "Включить")
        {
            var enabled = await _appState.SetNotificationsEnabledAsync(true);

            if (!enabled)
            {
                await DisplayAlertAsync("Разрешение не выдано", "Уведомления можно включить позже в настройках.", "ОК");
            }
        }

        await _navigateToHomeAsync();
    }
}
