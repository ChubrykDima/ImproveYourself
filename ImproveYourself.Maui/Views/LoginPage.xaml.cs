using ImproveYourself.Maui.Application;
using ImproveYourself.Maui.Resources.Strings;

namespace ImproveYourself.Maui.Views;

public partial class LoginPage : ContentPage
{
    private readonly AppState _appState;
    private bool _isSubmitting;

    public LoginPage(AppState appState)
    {
        InitializeComponent();
        _appState = appState;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        RenderStaticLabels();
    }

    private void RenderStaticLabels()
    {
        Title = AppStrings.AuthLoginTitle;
        TitleLabel.Text = AppStrings.AuthLoginTitle;
        DescriptionLabel.Text = AppStrings.AuthLoginDescription;
        EmailEntry.Placeholder = AppStrings.AuthEmailPlaceholder;
        PasswordEntry.Placeholder = AppStrings.AuthPasswordPlaceholder;
        LoginButton.Text = AppStrings.AuthLoginButton;
        RegisterButton.Text = AppStrings.AuthGoToRegister;
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        if (_isSubmitting)
        {
            return;
        }

        _isSubmitting = true;
        LoginButton.IsEnabled = false;
        StatusLabel.Text = AppStrings.AuthWorking;

        var result = await _appState.LoginAsync(
            EmailEntry.Text ?? string.Empty,
            PasswordEntry.Text ?? string.Empty);

        StatusLabel.Text = result.Message;

        if (result.Succeeded)
        {
            await Navigation.PopAsync();
        }

        LoginButton.IsEnabled = true;
        _isSubmitting = false;
    }

    private async void OnRegisterClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage(_appState));
    }
}
