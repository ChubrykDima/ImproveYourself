using System.Text.Json;
using System.Text.Json.Serialization;
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using ImproveYourself.Maui.Application;

namespace ImproveYourself.Maui;

/// <summary>
/// Schedules daily reminders with inexact repeating alarms (no exact-alarm permission needed).
/// The schedule is persisted so it can be restored after a device reboot, when the MAUI app
/// (and its localized strings) is not running.
/// </summary>
public static class ReminderScheduler
{
    public const string ChannelId = "daily_reminders";

    private const string PreferencesName = "improveyourself.reminders";
    private const string ScheduleKey = "schedule";
    private const string ExtraId = "reminder_id";
    private const string ExtraTitle = "reminder_title";
    private const string ExtraBody = "reminder_body";

    public static void Schedule(Context context, IReadOnlyList<ReminderDefinition> reminders, string? channelName)
    {
        Cancel(context);
        if (channelName is not null)
        {
            EnsureChannel(context, channelName);
        }

        var alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService)!;
        foreach (var reminder in reminders)
        {
            alarmManager.SetInexactRepeating(
                AlarmType.RtcWakeup,
                NextTriggerMillis(reminder.Hour, reminder.Minute),
                AlarmManager.IntervalDay,
                CreateAlarmIntent(context, reminder));
        }

        Store(context).Edit()!
            .PutString(ScheduleKey, JsonSerializer.Serialize(reminders.ToList(), ReminderJsonContext.Default.ListReminderDefinition))!
            .Apply();
    }

    public static void Cancel(Context context)
    {
        var alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService)!;
        foreach (var reminder in ReadSchedule(context))
        {
            alarmManager.Cancel(CreateAlarmIntent(context, reminder));
        }

        Store(context).Edit()!.Remove(ScheduleKey)!.Apply();
    }

    public static void Restore(Context context)
    {
        var reminders = ReadSchedule(context);
        if (reminders.Count > 0)
        {
            Schedule(context, reminders, channelName: null);
        }
    }

    public static void Show(Context context, Intent intent)
    {
        var id = intent.GetIntExtra(ExtraId, 0);
        var title = intent.GetStringExtra(ExtraTitle);
        var body = intent.GetStringExtra(ExtraBody);
        if (id == 0 || string.IsNullOrWhiteSpace(title))
        {
            return;
        }

        var manager = NotificationManagerCompat.From(context);
        if (!manager.AreNotificationsEnabled())
        {
            return;
        }

        var launchIntent = context.PackageManager!.GetLaunchIntentForPackage(context.PackageName!);
        var contentIntent = launchIntent is null
            ? null
            : PendingIntent.GetActivity(context, id, launchIntent, PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent);

        var notification = new NotificationCompat.Builder(context, ChannelId)
            .SetSmallIcon(context.ApplicationInfo!.Icon)!
            .SetContentTitle(title)!
            .SetContentText(body)!
            .SetStyle(new NotificationCompat.BigTextStyle().BigText(body))!
            .SetAutoCancel(true)!
            .SetContentIntent(contentIntent)!
            .Build()!;

        manager.Notify(id, notification);
    }

    private static PendingIntent CreateAlarmIntent(Context context, ReminderDefinition reminder)
    {
        var intent = new Intent(context, typeof(ReminderAlarmReceiver));
        intent.PutExtra(ExtraId, reminder.Id);
        intent.PutExtra(ExtraTitle, reminder.Title);
        intent.PutExtra(ExtraBody, reminder.Body);

        return PendingIntent.GetBroadcast(
            context,
            reminder.Id,
            intent,
            PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent)!;
    }

    private static long NextTriggerMillis(int hour, int minute)
    {
        var now = DateTimeOffset.Now;
        var next = new DateTimeOffset(now.Year, now.Month, now.Day, hour, minute, 0, now.Offset);
        if (next <= now)
        {
            next = next.AddDays(1);
        }

        return next.ToUnixTimeMilliseconds();
    }

    private static void EnsureChannel(Context context, string channelName)
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.O)
        {
            return;
        }

        var manager = (NotificationManager)context.GetSystemService(Context.NotificationService)!;
        manager.CreateNotificationChannel(
            new NotificationChannel(ChannelId, channelName, NotificationImportance.Default));
    }

    private static List<ReminderDefinition> ReadSchedule(Context context)
    {
        var json = Store(context).GetString(ScheduleKey, null);
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize(json, ReminderJsonContext.Default.ListReminderDefinition) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static ISharedPreferences Store(Context context) =>
        context.GetSharedPreferences(PreferencesName, FileCreationMode.Private)!;
}

[JsonSerializable(typeof(List<ReminderDefinition>))]
internal sealed partial class ReminderJsonContext : JsonSerializerContext;

[BroadcastReceiver(Enabled = true, Exported = false)]
public sealed class ReminderAlarmReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context is not null && intent is not null)
        {
            ReminderScheduler.Show(context, intent);
        }
    }
}

[BroadcastReceiver(Enabled = true, Exported = true)]
[IntentFilter(new[] { Intent.ActionBootCompleted, Intent.ActionMyPackageReplaced })]
public sealed class ReminderBootReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context is not null)
        {
            ReminderScheduler.Restore(context);
        }
    }
}
