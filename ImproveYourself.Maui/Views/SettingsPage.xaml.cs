using ImproveYourself.Maui.Application;

namespace ImproveYourself.Maui.Views;

public partial class SettingsPage : ContentPage
{
    private readonly AppState _appState;
    private bool _isSyncing;
    private bool _isUpdating;

    public SettingsPage(AppState appState)
    {
        InitializeComponent();
        _appState = appState;
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
}
