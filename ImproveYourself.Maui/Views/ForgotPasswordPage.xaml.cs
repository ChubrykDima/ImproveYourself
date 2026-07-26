using ImproveYourself.Maui.Application;
using ImproveYourself.Maui.Resources.Strings;

namespace ImproveYourself.Maui.Views;

public partial class ForgotPasswordPage : ContentPage
{
    private readonly AppState _appState;
    private bool _isSubmitting;
    private bool _isConfirming;

    public ForgotPasswordPage(AppState appState, string? prefillEmail = null)
    {
        InitializeComponent();
        _appState = appState;
        EmailEntry.Text = prefillEmail ?? string.Empty;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Title = AppStrings.AuthForgotPasswordTitle;
        TitleLabel.Text = AppStrings.AuthForgotPasswordTitle;
        DescriptionLabel.Text = AppStrings.AuthForgotPasswordDescription;
        EmailEntry.Placeholder = AppStrings.AuthEmailPlaceholder;
        SubmitButton.Text = AppStrings.AuthForgotPasswordSubmit;
        ShowConfirmButton.Text = AppStrings.AuthResetHaveCodeButton;
        ConfirmTitleLabel.Text = AppStrings.AuthResetConfirmTitle;
        ConfirmDescriptionLabel.Text = AppStrings.AuthResetConfirmDescription;
        TokenEntry.Placeholder = AppStrings.AuthResetTokenPlaceholder;
        NewPasswordEntry.Placeholder = AppStrings.AuthNewPasswordPlaceholder;
        ConfirmButton.Text = AppStrings.AuthResetConfirmSubmit;
    }

    private async void OnSubmitClicked(object? sender, EventArgs e)
    {
        if (_isSubmitting)
        {
            return;
        }

        _isSubmitting = true;
        SubmitButton.IsEnabled = false;
        StatusLabel.Text = AppStrings.AuthWorking;

        try
        {
            var result = await _appState.RequestPasswordResetAsync(EmailEntry.Text ?? string.Empty);
            StatusLabel.Text = result.Message;

            if (result.Succeeded)
            {
                // Always replace the token field: a new request revokes prior unused codes.
                // In production the API does not echo ResetToken, so clear any stale pasted value.
                ShowConfirmSection(result.ResetToken, replaceTokenField: true);
                await DisplayAlertAsync(AppStrings.AuthForgotPasswordTitle, result.Message, AppStrings.OK);
            }
            else if (result.BackendEndpointMissing)
            {
                await DisplayAlertAsync(AppStrings.AuthForgotPasswordTitle, result.Message, AppStrings.OK);
            }
        }
        finally
        {
            SubmitButton.IsEnabled = true;
            _isSubmitting = false;
        }
    }

    private void OnShowConfirmClicked(object? sender, EventArgs e)
    {
        ShowConfirmSection(prefillToken: null);
    }

    private async void OnConfirmClicked(object? sender, EventArgs e)
    {
        if (_isConfirming)
        {
            return;
        }

        _isConfirming = true;
        ConfirmButton.IsEnabled = false;
        ConfirmStatusLabel.Text = AppStrings.AuthWorking;

        try
        {
            var result = await _appState.ConfirmPasswordResetAsync(
                EmailEntry.Text ?? string.Empty,
                TokenEntry.Text ?? string.Empty,
                NewPasswordEntry.Text ?? string.Empty);

            ConfirmStatusLabel.Text = result.Message;

            if (result.Succeeded)
            {
                await DisplayAlertAsync(AppStrings.AuthResetConfirmTitle, result.Message, AppStrings.OK);
                await Navigation.PopAsync();
            }
            else if (result.BackendEndpointMissing)
            {
                await DisplayAlertAsync(AppStrings.AuthResetConfirmTitle, result.Message, AppStrings.OK);
            }
        }
        finally
        {
            ConfirmButton.IsEnabled = true;
            _isConfirming = false;
        }
    }

    private void ShowConfirmSection(string? prefillToken, bool replaceTokenField = false)
    {
        ConfirmBorder.IsVisible = true;

        if (replaceTokenField)
        {
            TokenEntry.Text = prefillToken ?? string.Empty;
        }
        else if (!string.IsNullOrWhiteSpace(prefillToken))
        {
            TokenEntry.Text = prefillToken;
        }
    }
}
