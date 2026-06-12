namespace ImproveYourself.Maui.Application;

public interface INotificationPreferenceService
{
    Task<bool> ApplyPreferenceAsync(bool enabled);
}
