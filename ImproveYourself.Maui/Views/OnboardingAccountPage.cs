using ImproveYourself.Maui.Application;
using ImproveYourself.Maui.Domain;
using ImproveYourself.Maui.Resources.Strings;

namespace ImproveYourself.Maui.Views;

public sealed class OnboardingAccountPage : ContentPage
{
    private readonly AppState _appState;
    private readonly Func<Task> _navigateToHomeAsync;
    private readonly string _displayName;
    private readonly Button _createButton;
    private readonly Button _signInButton;
    private readonly Button _skipButton;
    private bool _waitingForAuth;
    private bool _advanced;

    public OnboardingAccountPage(AppState appState, Func<Task> navigateToHomeAsync, string displayName)
    {
        _appState = appState;
        _navigateToHomeAsync = navigateToHomeAsync;
        _displayName = displayName;
        BackgroundColor = Color.FromArgb("#090C12");
        Padding = new Thickness(22, 54, 22, 30);

        var title = new Label
        {
            Text = AppStrings.OnboardingAccount_Title,
            Style = (Style)Microsoft.Maui.Controls.Application.Current!.Resources["HeroLabelStyle"],
        };
        var body = new Label
        {
            Text = AppStrings.OnboardingAccount_Body,
            Style = (Style)Microsoft.Maui.Controls.Application.Current.Resources["BodyLabelStyle"],
            HorizontalTextAlignment = TextAlignment.Center,
        };
        _createButton = new Button
        {
            Text = AppStrings.OnboardingAccount_Create,
            Style = (Style)Microsoft.Maui.Controls.Application.Current.Resources["PrimaryButtonStyle"],
        };
        _signInButton = new Button
        {
            Text = AppStrings.OnboardingAccount_SignIn,
            Style = (Style)Microsoft.Maui.Controls.Application.Current.Resources["GhostButtonStyle"],
        };
        _skipButton = new Button
        {
            Text = AppStrings.OnboardingAccount_Skip,
            BackgroundColor = Colors.Transparent,
            TextColor = Color.FromArgb("#9EA8BD"),
            FontFamily = "OpenSansRegular",
            FontSize = 15,
        };

        _createButton.Clicked += async (_, _) => await OpenAuthAsync(new RegisterPage(_appState));
        _signInButton.Clicked += async (_, _) => await OpenAuthAsync(new LoginPage(_appState));
        _skipButton.Clicked += async (_, _) => await ContinueAsync();

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Spacing = 16,
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    title,
                    body,
                    _createButton,
                    _signInButton,
                    _skipButton,
                },
            },
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_waitingForAuth && _appState.IsLoggedIn)
        {
            _waitingForAuth = false;
            await ContinueAsync();
            return;
        }

        _waitingForAuth = false;
        _advanced = false;
        _createButton.IsEnabled = true;
        _signInButton.IsEnabled = true;
        _skipButton.IsEnabled = true;

        if (_appState.IsLoggedIn)
        {
            _createButton.IsVisible = false;
            _signInButton.IsVisible = false;
            _skipButton.Text = AppStrings.OnboardingAccount_Continue;
        }
    }

    private async Task OpenAuthAsync(Page page)
    {
        if (_advanced || _waitingForAuth)
        {
            return;
        }

        _waitingForAuth = true;
        _createButton.IsEnabled = false;
        _signInButton.IsEnabled = false;
        _skipButton.IsEnabled = false;
        await Navigation.PushAsync(page);
    }

    private async Task ContinueAsync()
    {
        if (_advanced)
        {
            return;
        }

        _advanced = true;
        _createButton.IsEnabled = false;
        _signInButton.IsEnabled = false;
        _skipButton.IsEnabled = false;

        await Navigation.PushAsync(new SelfAssessmentPage(
            _appState,
            SelfAssessmentKind.Start,
            async () =>
            {
                await _appState.CompleteOnboardingAsync(_displayName);
                await _navigateToHomeAsync();
            },
            promptNotificationsOnCompletion: true));
    }
}
