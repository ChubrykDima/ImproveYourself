namespace ImproveYourself.Maui.Application;

public sealed class NotificationPreferenceService : INotificationPreferenceService
{
    public Task<bool> ApplyPreferenceAsync(bool enabled)
    {
        if (!enabled)
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }
}
