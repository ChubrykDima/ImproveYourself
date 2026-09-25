using AndroidX.Core.App;
using ImproveYourself.Maui.Resources.Strings;
using Microsoft.Maui.ApplicationModel;

namespace ImproveYourself.Maui.Application;

public sealed partial class NotificationPreferenceService
{
    private static partial async Task<bool> HasPermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
        return status == PermissionStatus.Granted
            && NotificationManagerCompat.From(global::Android.App.Application.Context).AreNotificationsEnabled();
    }

    private static partial async Task<bool> RequestPermissionAsync()
    {
        var status = await MainThread.InvokeOnMainThreadAsync(
            async () => await Permissions.RequestAsync<Permissions.PostNotifications>());

        return status == PermissionStatus.Granted
            && NotificationManagerCompat.From(global::Android.App.Application.Context).AreNotificationsEnabled();
    }

    private static partial Task ScheduleRemindersAsync(IReadOnlyList<ReminderDefinition> reminders)
    {
        ReminderScheduler.Schedule(global::Android.App.Application.Context, reminders, AppStrings.Reminders);
        return Task.CompletedTask;
    }

    private static partial Task CancelRemindersAsync()
    {
        ReminderScheduler.Cancel(global::Android.App.Application.Context);
        return Task.CompletedTask;
    }
}
