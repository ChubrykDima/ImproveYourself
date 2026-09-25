namespace ImproveYourself.Maui.Application;

public sealed partial class NotificationPreferenceService
{
    private static partial Task<bool> HasPermissionAsync() => Task.FromResult(false);

    private static partial Task<bool> RequestPermissionAsync() => Task.FromResult(false);

    private static partial Task ScheduleRemindersAsync(IReadOnlyList<ReminderDefinition> reminders) => Task.CompletedTask;

    private static partial Task CancelRemindersAsync() => Task.CompletedTask;
}
