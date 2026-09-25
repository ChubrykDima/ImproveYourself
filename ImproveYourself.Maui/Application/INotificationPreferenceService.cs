namespace ImproveYourself.Maui.Application;

public interface INotificationPreferenceService
{
    /// <summary>
    /// Enables (requesting OS permission if needed) or disables daily reminders.
    /// Returns whether reminders are active afterwards.
    /// </summary>
    Task<bool> ApplyPreferenceAsync(bool enabled);

    /// <summary>
    /// Reschedules reminders with the current language without prompting for permission.
    /// Returns false when the OS permission has been revoked.
    /// </summary>
    Task<bool> RefreshAsync(bool enabled);
}
