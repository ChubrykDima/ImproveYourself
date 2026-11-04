namespace ImproveYourself.Maui.Views;

internal static class AuthFormScroll
{
    public static async Task RevealStatusAsync(ScrollView scroll, VisualElement status)
    {
        await WaitForKeyboardToCloseAsync();
        await scroll.Dispatcher.DispatchAsync(() => { });

        if (Math.Abs(scroll.ScrollX) > 0.5)
        {
            await scroll.ScrollToAsync(0, Math.Max(scroll.ScrollY, 0), false);
        }

        if (IsFullyVisible(scroll, status))
        {
            return;
        }

        await scroll.ScrollToAsync(status, ScrollToPosition.MakeVisible, false);

        if (Math.Abs(scroll.ScrollX) > 0.5)
        {
            await scroll.ScrollToAsync(0, Math.Max(scroll.ScrollY, 0), false);
        }
    }

    private static bool IsFullyVisible(ScrollView scroll, VisualElement element)
    {
        if (scroll.Height <= 0 || element.Height <= 0 || scroll.Content is not VisualElement content)
        {
            // A short form stays on screen once the keyboard is gone. Scrolling
            // without a measurement is what shifts the card.
            return true;
        }

        var top = GetY(element, content);
        var bottom = top + element.Height;
        var visibleTop = scroll.ScrollY;
        var visibleBottom = scroll.ScrollY + scroll.Height;

        return top >= visibleTop - 1 && bottom <= visibleBottom + 1;
    }

    private static double GetY(VisualElement element, VisualElement root)
    {
        double y = 0;
        VisualElement? current = element;

        while (current is not null && current != root)
        {
            y += current.Y;
            current = current.Parent as VisualElement;
            if (current is null)
            {
                break;
            }
        }

        return y;
    }

    private static async Task WaitForKeyboardToCloseAsync()
    {
#if IOS || MACCATALYST
        if (!IsKeyboardVisible())
        {
            return;
        }

        var hidden = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var token = UIKit.UIKeyboard.Notifications.ObserveDidHide((_, _) => hidden.TrySetResult());
        await Task.WhenAny(hidden.Task, Task.Delay(TimeSpan.FromMilliseconds(700)));
        await Task.Delay(32);
#else
        await Task.CompletedTask;
#endif
    }

#if IOS || MACCATALYST
    private static bool IsKeyboardVisible()
    {
        foreach (var scene in UIKit.UIApplication.SharedApplication.ConnectedScenes)
        {
            if (scene is not UIKit.UIWindowScene windowScene)
            {
                continue;
            }

            foreach (var window in windowScene.Windows)
            {
                var name = window.Class.Name.ToString();
                if (!name.Contains("Keyboard", StringComparison.Ordinal))
                {
                    continue;
                }

                var screenHeight = (double)windowScene.Screen.Bounds.Height;
                if (!window.Hidden
                    && (double)window.Alpha > 0.01
                    && (double)window.Frame.Height > 1
                    && (double)window.Frame.Y < screenHeight - 1)
                {
                    return true;
                }
            }
        }

        return false;
    }
#endif
}
