using ImproveYourself.Maui.Application;

namespace ImproveYourself.Maui.Views;

public partial class SettingsPage : ContentPage
{
    private readonly AppState _appState;
    private readonly IBackendConnectionService _backendConnectionService;
    private bool _isSyncing;
    private bool _isUpdating;
    private bool _isCheckingBackend;

    public SettingsPage(AppState appState, IBackendConnectionService backendConnectionService)
    {
        InitializeComponent();
        _appState = appState;
        _backendConnectionService = backendConnectionService;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        SyncFromState();
    }

    private void SyncFromState()
    {
        _isSyncing = true;
        NameEntry.Text = _appState.DisplayName;
        NotificationSwitch.IsToggled = _appState.NotificationsEnabled;
        BackendUrlEntry.Text = _appState.BackendBaseUrl;
        BackendApiKeyEntry.Text = _appState.BackendApiKey;
        BackendStatusLabel.Text = string.IsNullOrWhiteSpace(_appState.BackendBaseUrl)
            ? "Backend не настроен."
            : "Backend сохранен. Можно проверить подключение.";
        _isSyncing = false;
    }

    private async void OnSaveNameClicked(object? sender, EventArgs e)
    {
        _appState.UpdateDisplayName(NameEntry.Text ?? string.Empty);
        await DisplayAlertAsync("Сохранено", "Имя обновлено.", "ОК");
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
            await DisplayAlertAsync("Нет доступа к уведомлениям", "Разрешение можно изменить в настройках устройства.", "ОК");
        }

        _isSyncing = true;
        NotificationSwitch.IsToggled = applied;
        _isSyncing = false;

        NotificationSwitch.IsEnabled = true;
        _isUpdating = false;
    }

    private async void OnSaveBackendClicked(object? sender, EventArgs e)
    {
        _appState.UpdateBackendConnection(
            BackendUrlEntry.Text ?? string.Empty,
            BackendApiKeyEntry.Text ?? string.Empty);

        SyncFromState();
        await DisplayAlertAsync("Сохранено", "Настройки backend обновлены.", "ОК");
    }

    private async void OnCheckBackendClicked(object? sender, EventArgs e)
    {
        if (_isCheckingBackend)
        {
            return;
        }

        _appState.UpdateBackendConnection(
            BackendUrlEntry.Text ?? string.Empty,
            BackendApiKeyEntry.Text ?? string.Empty);

        _isCheckingBackend = true;
        CheckBackendButton.IsEnabled = false;
        BackendStatusLabel.Text = "Проверяем backend...";

        var result = await _backendConnectionService.CheckAsync();
        BackendStatusLabel.Text = result.Message;

        CheckBackendButton.IsEnabled = true;
        _isCheckingBackend = false;
    }
}
