namespace ImproveYourself.Maui.Views;

internal static class OnboardingNavigationGate
{
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

    public static void Exit()
    {
        _busy = false;
        var callback = _whenIdle;
        _whenIdle = null;
        callback?.Invoke();
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
