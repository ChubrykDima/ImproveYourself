using ImproveYourself.Maui.Legal;
using ImproveYourself.Maui.Resources.Strings;

namespace ImproveYourself.Maui.Views;

public partial class LegalDocumentPage : ContentPage
{
    private readonly string _title;
    private readonly string _body;
    private readonly string? _externalUrl;

    public static LegalDocumentPage PrivacyPolicy() =>
        new(
            AppStrings.LegalPrivacyTitle,
            LegalDocuments.PrivacyPolicyBody,
            LegalUrls.HasPrivacyPolicyUrl ? LegalUrls.PrivacyPolicy : null);

    public static LegalDocumentPage TermsOfService() =>
        new(
            AppStrings.LegalTermsTitle,
            LegalDocuments.TermsOfServiceBody,
            LegalUrls.HasTermsOfServiceUrl ? LegalUrls.TermsOfService : null);

    private LegalDocumentPage(string title, string body, string? externalUrl)
    {
        InitializeComponent();
        _title = title;
        _body = body;
        _externalUrl = externalUrl;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Title = _title;
        TitleLabel.Text = _title;
        BodyLabel.Text = _body;
        OpenInBrowserButton.Text = AppStrings.LegalOpenInBrowser;
        OpenInBrowserButton.IsVisible = !string.IsNullOrWhiteSpace(_externalUrl);
    }

    private async void OnOpenInBrowserClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_externalUrl)
            || !Uri.TryCreate(_externalUrl, UriKind.Absolute, out var uri))
        {
            return;
        }

        try
        {
            await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        }
        catch
        {
            await DisplayAlertAsync(AppStrings.LegalPrivacyTitle, AppStrings.LegalOpenFailed, AppStrings.OK);
        }
    }
}
