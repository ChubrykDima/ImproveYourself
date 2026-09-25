using ImproveYourself.Maui.Resources.Strings;

namespace ImproveYourself.Maui.Application;

public sealed record ReminderDefinition(int Id, string Identifier, int Hour, int Minute, string Title, string Body);

public sealed partial class NotificationPreferenceService : INotificationPreferenceService
{
    private const string MorningIdentifier = "improveyourself.reminder.morning";
    private const string EveningIdentifier = "improveyourself.reminder.evening";

    private static readonly string[] ReminderIdentifiers = [MorningIdentifier, EveningIdentifier];

    public async Task<bool> ApplyPreferenceAsync(bool enabled)
    {
        if (!enabled)
        {
            await CancelRemindersAsync();
            return false;
        }

        if (!await RequestPermissionAsync())
        {
            await CancelRemindersAsync();
            return false;
        }

        await ScheduleRemindersAsync(BuildReminders());
        return true;
    }

    public async Task<bool> RefreshAsync(bool enabled)
    {
        if (!enabled || !await HasPermissionAsync())
        {
            await CancelRemindersAsync();
            return false;
        }

        await ScheduleRemindersAsync(BuildReminders());
        return true;
    }

    private static IReadOnlyList<ReminderDefinition> BuildReminders() =>
    [
        new(1001, MorningIdentifier, 9, 0, AppStrings.ReminderMorningTitle, AppStrings.ReminderMorningBody),
        new(1002, EveningIdentifier, 20, 0, AppStrings.ReminderEveningTitle, AppStrings.ReminderEveningBody),
    ];

    private static partial Task<bool> HasPermissionAsync();

    private static partial Task<bool> RequestPermissionAsync();

    private static partial Task ScheduleRemindersAsync(IReadOnlyList<ReminderDefinition> reminders);

    private static partial Task CancelRemindersAsync();
}
