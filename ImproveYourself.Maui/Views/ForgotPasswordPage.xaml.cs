using ImproveYourself.Maui.Application;
using ImproveYourself.Maui.Resources.Strings;

namespace ImproveYourself.Maui.Views;

public partial class ForgotPasswordPage : ContentPage
{
    private readonly AppState _appState;
    private bool _isSubmitting;

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

        var result = await _appState.RequestPasswordResetAsync(EmailEntry.Text ?? string.Empty);
        StatusLabel.Text = result.Message;

        if (result.Succeeded)
        {
            await DisplayAlertAsync(AppStrings.AuthForgotPasswordTitle, result.Message, AppStrings.OK);
            await Navigation.PopAsync();
        }
        else if (result.BackendEndpointMissing)
        {
            await DisplayAlertAsync(AppStrings.AuthForgotPasswordTitle, result.Message, AppStrings.OK);
        }

        SubmitButton.IsEnabled = true;
        _isSubmitting = false;
    }
}
