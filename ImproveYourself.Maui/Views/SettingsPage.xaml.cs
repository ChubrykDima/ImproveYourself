using ImproveYourself.Maui;
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
        BackendSettingsBorder.IsVisible = BackendDefaults.AllowManualBackendSettings;
        NameEntry.Text = _appState.DisplayName;
        NotificationSwitch.IsToggled = _appState.NotificationsEnabled;
        BackendUrlEntry.Text = _appState.BackendBaseUrl;
        BackendApiKeyEntry.Text = BackendDefaults.AllowManualBackendSettings
            ? _appState.BackendApiKey
            : string.Empty;
        BackendStatusLabel.Text = !string.IsNullOrWhiteSpace(_appState.BackendSyncMessage)
            ? _appState.BackendSyncMessage
            : string.IsNullOrWhiteSpace(_appState.BackendBaseUrl)
                ? "Backend не настроен."
                : "Backend сохранен. Можно проверить подключение и синхронизацию.";
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

        if (result.HealthOk && result.ReadyOk && result.AuthorizationOk)
        {
            BackendStatusLabel.Text = "Backend проверен. Синхронизируем прогресс...";
            var syncResult = await _appState.SyncBackendAsync(force: true);
            BackendStatusLabel.Text = $"{result.Message}\n{syncResult.Message}";
        }

        CheckBackendButton.IsEnabled = true;
        _isCheckingBackend = false;
    }
}
