using System.ComponentModel;
using ImproveYourself.Maui;
using ImproveYourself.Maui.Application;
using ImproveYourself.Maui.Resources.Strings;

namespace ImproveYourself.Maui.Views;

public partial class SettingsPage : ContentPage
{
    private static readonly (string Code, string Label)[] LanguageOptions =
    [
        ("en", "English"),
        ("ru", "Русский"),
        ("de", "Deutsch"),
    ];

    private readonly AppState _appState;
    private readonly IBackendConnectionService _backendConnectionService;
    private readonly ILocalizationService _localizationService;
    private bool _isSyncing;
    private bool _isUpdating;
    private bool _isCheckingBackend;
    private readonly PropertyChangedEventHandler _appStatePropertyChangedHandler;

    public SettingsPage(AppState appState, IBackendConnectionService backendConnectionService, ILocalizationService localizationService)
    {
        InitializeComponent();
        _appState = appState;
        _backendConnectionService = backendConnectionService;
        _localizationService = localizationService;
        _appStatePropertyChangedHandler = OnAppStatePropertyChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _appState.PropertyChanged += _appStatePropertyChangedHandler;
        RenderStaticLabels();
        SyncFromState();
    }

    protected override void OnDisappearing()
    {
        _appState.PropertyChanged -= _appStatePropertyChangedHandler;
        base.OnDisappearing();
    }

    private void OnAppStatePropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName is nameof(AppState.IsLoggedIn) or nameof(AppState.UserEmail) or nameof(AppState.BackendSyncMessage))
        {
            MainThread.BeginInvokeOnMainThread(SyncFromState);
        }
    }

    private void RenderStaticLabels()
    {
        Title = AppStrings.Settings_Title;
        ProfileSectionLabel.Text = AppStrings.Profile;
        GreetingNameLabel.Text = AppStrings.GreetingName;
        NameEntry.Placeholder = AppStrings.EnterNamePlaceholder;
        SaveNameButton.Text = AppStrings.SaveName;
        LanguageSectionLabel.Text = AppStrings.LanguageSection;
        LanguagePickerButton.Text = AppStrings.LanguageSection;
        RemindersSectionLabel.Text = AppStrings.Reminders;
        ReminderDescLabel.Text = AppStrings.ReminderDescription;
        EnableNotificationsLabel.Text = AppStrings.EnableNotifications;
        OfflineFirstTitleLabel.Text = AppStrings.OfflineFirstTitle;
        OfflineFirstDescLabel.Text = AppStrings.OfflineFirstDescription;
        AccountSectionLabel.Text = AppStrings.AuthAccountSection;
        AccountDescLabel.Text = AppStrings.AuthAccountDescription;
        LoginButton.Text = AppStrings.AuthLoginButton;
        RegisterAccountButton.Text = AppStrings.AuthRegisterButton;
        LogoutButton.Text = AppStrings.AuthLogoutButton;
        SyncButton.Text = AppStrings.AuthSyncButton;
        BackendSectionLabel.Text = AppStrings.Backend;
        BackendDescLabel.Text = AppStrings.BackendDescription;
        SaveBackendButton.Text = AppStrings.SaveButton;
        CheckBackendButton.Text = AppStrings.CheckButton;
    }

    private void SyncFromState()
    {
        _isSyncing = true;
        BackendSettingsBorder.IsVisible = BackendDefaults.AllowManualBackendSettings;
        NameEntry.Text = _appState.DisplayName;
        NotificationSwitch.IsToggled = _appState.NotificationsEnabled;
        BackendUrlEntry.Text = _appState.BackendBaseUrl;

        if (_appState.IsLoggedIn)
        {
            AccountStatusLabel.Text = string.Format(AppStrings.AuthLoggedInAsFormat, _appState.UserEmail);
            LoginButton.IsVisible = false;
            RegisterAccountButton.IsVisible = false;
            LogoutButton.IsVisible = true;
            SyncButton.IsVisible = true;
        }
        else
        {
            AccountStatusLabel.Text = AppStrings.AuthNotLoggedIn;
            LoginButton.IsVisible = true;
            RegisterAccountButton.IsVisible = true;
            LogoutButton.IsVisible = false;
            SyncButton.IsVisible = false;
        }

        BackendStatusLabel.Text = !string.IsNullOrWhiteSpace(_appState.BackendSyncMessage)
            ? _appState.BackendSyncMessage
            : string.IsNullOrWhiteSpace(_appState.BackendBaseUrl)
                ? AppStrings.BackendNotConfigured
                : AppStrings.BackendSaved;

        var currentLanguage = LanguageOptions.FirstOrDefault(l => l.Code == _localizationService.CurrentLanguage);
        LanguageCurrentLabel.Text = currentLanguage.Label ?? _localizationService.CurrentLanguage;
        _isSyncing = false;
    }

    private async void OnSaveNameClicked(object? sender, EventArgs e)
    {
        _appState.UpdateDisplayName(NameEntry.Text ?? string.Empty);
        await DisplayAlertAsync(AppStrings.Saved_Title, AppStrings.NameUpdated, AppStrings.OK);
    }

    private async void OnNotificationToggled(object? sender, ToggledEventArgs e)
    {
        if (_isSyncing || _isUpdating)
        {
            return;
        }

        _isUpdating = true;
        NotificationSwitch.IsEnabled = false;

        var applied = await _appState.SetNotificationsEnabledAsync(e.Value);

        if (e.Value && !applied)
        {
            await DisplayAlertAsync(AppStrings.NoNotificationAccess, AppStrings.NotificationPermissionInstructions, AppStrings.OK);
        }

        _isSyncing = true;
        NotificationSwitch.IsToggled = applied;
        _isSyncing = false;

        NotificationSwitch.IsEnabled = true;
        _isUpdating = false;
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new LoginPage(_appState));
    }

    private async void OnRegisterAccountClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage(_appState));
    }

    private async void OnLogoutClicked(object? sender, EventArgs e)
    {
        await _appState.LogoutAsync();
        SyncFromState();
    }

    private async void OnSyncClicked(object? sender, EventArgs e)
    {
        BackendStatusLabel.Text = AppStrings.BackendVerifiedSyncing;
        var syncResult = await _appState.SyncBackendAsync(force: true);
        BackendStatusLabel.Text = syncResult.Message;
    }

    private async void OnSaveBackendClicked(object? sender, EventArgs e)
    {
        _appState.UpdateBackendBaseUrl(BackendUrlEntry.Text ?? string.Empty);
        SyncFromState();
        await DisplayAlertAsync(AppStrings.Saved_Title, AppStrings.BackendSettingsUpdated, AppStrings.OK);
    }

    private async void OnCheckBackendClicked(object? sender, EventArgs e)
    {
        if (_isCheckingBackend)
        {
            return;
        }

        _appState.UpdateBackendBaseUrl(BackendUrlEntry.Text ?? string.Empty);

        _isCheckingBackend = true;
        CheckBackendButton.IsEnabled = false;
        BackendStatusLabel.Text = AppStrings.CheckingBackend;

        var result = await _backendConnectionService.CheckAsync();
        BackendStatusLabel.Text = result.Message;

        if (result.HealthOk && result.ReadyOk && result.AuthorizationOk)
        {
            BackendStatusLabel.Text = AppStrings.BackendVerifiedSyncing;
            var syncResult = await _appState.SyncBackendAsync(force: true);
            BackendStatusLabel.Text = $"{result.Message}\n{syncResult.Message}";
        }

        CheckBackendButton.IsEnabled = true;
        _isCheckingBackend = false;
    }

    private async void OnLanguagePickerClicked(object? sender, EventArgs e)
    {
        var labels = LanguageOptions.Select(l => l.Label).ToArray();
        var choice = await DisplayActionSheetAsync(AppStrings.LanguageSection, AppStrings.Back, null, labels);

        if (string.IsNullOrEmpty(choice) || choice == AppStrings.Back)
        {
            return;
        }

        var selected = LanguageOptions.FirstOrDefault(l => l.Label == choice);

        if (string.IsNullOrEmpty(selected.Code) || selected.Code == _localizationService.CurrentLanguage)
        {
            return;
        }

        _localizationService.SetLanguage(selected.Code);

        if (Microsoft.Maui.Controls.Application.Current is App app)
        {
            app.ReloadNavigation();
        }
    }
}
