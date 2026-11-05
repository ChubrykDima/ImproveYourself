using Microsoft.Maui.ApplicationModel;

namespace ImproveYourself.Maui.Views;

internal static class OnboardingNavigationGate
{
    // PushAsync/PopAsync return before the iOS transition finishes. Hold the gate
    // through that animation so a queued tap cannot enter the next page.
    private static readonly TimeSpan TransitionSettle = TimeSpan.FromMilliseconds(350);

    private static bool _busy;
    private static Action? _whenIdle;

    public static bool TryEnter()
    {
        if (_busy)
        {
            return false;
        }

        _busy = true;
        return true;
    }

    public static async Task ExitAfterTransitionAsync()
    {
        try
        {
            await Task.Delay(TransitionSettle);
        }
        finally
        {
            _busy = false;
            var callback = _whenIdle;
            _whenIdle = null;
            Invoke(callback);
        }
    }

    private static void Invoke(Action? callback)
    {
        if (callback is null)
        {
            return;
        }

        if (MainThread.IsMainThread)
        {
            callback();
            return;
        }

        MainThread.BeginInvokeOnMainThread(callback);
    }

    public static void RunWhenIdle(Action action)
    {
        if (!_busy)
        {
            action();
            return;
        }

        _whenIdle += action;
    }
}
