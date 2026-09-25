#if IOS || MACCATALYST
using Foundation;
using UserNotifications;

namespace ImproveYourself.Maui.Application;

public sealed partial class NotificationPreferenceService
{
    private static partial async Task<bool> HasPermissionAsync()
    {
        var settings = await UNUserNotificationCenter.Current.GetNotificationSettingsAsync();
        return IsAuthorized(settings.AuthorizationStatus);
    }

    private static partial async Task<bool> RequestPermissionAsync()
    {
        var center = UNUserNotificationCenter.Current;
        var settings = await center.GetNotificationSettingsAsync();

        if (IsAuthorized(settings.AuthorizationStatus))
        {
            return true;
        }

        if (settings.AuthorizationStatus == UNAuthorizationStatus.Denied)
        {
            return false;
        }

        var result = await center.RequestAuthorizationAsync(
            UNAuthorizationOptions.Alert | UNAuthorizationOptions.Sound);

        return result.Item1;
    }

    private static partial async Task ScheduleRemindersAsync(IReadOnlyList<ReminderDefinition> reminders)
    {
        var center = UNUserNotificationCenter.Current;
        center.RemovePendingNotificationRequests(ReminderIdentifiers);

        foreach (var reminder in reminders)
        {
            var content = new UNMutableNotificationContent
            {
                Title = reminder.Title,
                Body = reminder.Body,
                Sound = UNNotificationSound.Default,
            };

            var trigger = UNCalendarNotificationTrigger.CreateTrigger(
                new NSDateComponents { Hour = reminder.Hour, Minute = reminder.Minute },
                repeats: true);

            await center.AddNotificationRequestAsync(
                UNNotificationRequest.FromIdentifier(reminder.Identifier, content, trigger));
        }
    }

    private static partial Task CancelRemindersAsync()
    {
        UNUserNotificationCenter.Current.RemovePendingNotificationRequests(ReminderIdentifiers);
        return Task.CompletedTask;
    }

    private static bool IsAuthorized(UNAuthorizationStatus status) =>
        status is UNAuthorizationStatus.Authorized
            or UNAuthorizationStatus.Provisional
            or UNAuthorizationStatus.Ephemeral;
}
#endif
