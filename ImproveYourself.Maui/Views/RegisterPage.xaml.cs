using ImproveYourself.Maui.Application;
using ImproveYourself.Maui.Domain;
using ImproveYourself.Maui.Resources.Strings;

namespace ImproveYourself.Maui.Views;

public partial class RegisterPage : ContentPage
{
    private readonly AppState _appState;
    private bool _isSubmitting;

    public RegisterPage(AppState appState)
    {
        InitializeComponent();
        _appState = appState;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        RenderStaticLabels();
        PasswordField.Configure(PasswordEntry);
    }

    private void RenderStaticLabels()
    {
        Title = AppStrings.AuthRegisterTitle;
        TitleLabel.Text = AppStrings.AuthRegisterTitle;
        DescriptionLabel.Text = AppStrings.AuthRegisterDescription;
        EmailEntry.Placeholder = AppStrings.AuthEmailPlaceholder;
        PasswordEntry.Placeholder = AppStrings.AuthPasswordPlaceholder;
        PasswordRulesLabel.Text = AppStrings.AuthPasswordRules;
        RegisterButton.Text = AppStrings.AuthRegisterButton;
        LegalHintLabel.Text = AppStrings.AuthRegisterLegalHint;
        PrivacyPolicyButton.Text = AppStrings.LegalPrivacyTitle;
        TermsButton.Text = AppStrings.LegalTermsTitle;
    }

    private async void OnRegisterClicked(object? sender, EventArgs e)
    {
        if (_isSubmitting)
        {
            return;
        }

        var validation = AccountInput.ValidateRegistration(EmailEntry.Text, PasswordEntry.Text);

        if (validation is not null)
        {
            StatusLabel.Text = validation;
            EmailEntry.Unfocus();
            PasswordEntry.Unfocus();
            await FormScroll.ScrollToAsync(StatusLabel, ScrollToPosition.End, true);
            return;
        }

        _isSubmitting = true;
        RegisterButton.IsEnabled = false;
        StatusLabel.Text = AppStrings.AuthWorking;

        var result = await _appState.RegisterAsync(
            EmailEntry.Text ?? string.Empty,
            PasswordEntry.Text ?? string.Empty);

        StatusLabel.Text = result.Message;
        EmailEntry.Unfocus();
        PasswordEntry.Unfocus();
        await FormScroll.ScrollToAsync(StatusLabel, ScrollToPosition.End, true);

        if (result.Succeeded)
        {
            await Navigation.PopAsync();
            if (Navigation.NavigationStack.LastOrDefault() is LoginPage)
            {
                await Navigation.PopAsync();
            }
        }

        RegisterButton.IsEnabled = true;
        _isSubmitting = false;
    }

    private async void OnPrivacyPolicyClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(LegalDocumentPage.PrivacyPolicy());
    }

    private async void OnTermsClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(LegalDocumentPage.TermsOfService());
    }
}
