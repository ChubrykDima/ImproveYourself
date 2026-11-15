using Android.Content;
using Android.Views;
using ImproveYourself.Maui.Views;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace ImproveYourself.Maui;

public sealed class TapDismissScrollViewHandler : ScrollViewHandler
{
    protected override MauiScrollView CreatePlatformView() => new ObservingScrollView(
        new ContextThemeWrapper(Context, Microsoft.Maui.Resource.Style.scrollViewTheme), this)
    {
        ClipToOutline = true,
        FillViewport = true,
    };

    private sealed class ObservingScrollView(Context context, TapDismissScrollViewHandler handler)
        : MauiScrollView(context, null!, Microsoft.Maui.Resource.Attribute.scrollViewStyle)
    {
        private readonly int _touchSlop = ViewConfiguration.Get(context)!.ScaledTouchSlop;
        private float _downX;
        private float _downY;
        private bool _isTap;

        public override bool DispatchTouchEvent(MotionEvent? e)
        {
            if (e is null) return base.DispatchTouchEvent(e);

            if (e.ActionMasked == MotionEventActions.Down)
            {
                _downX = e.RawX;
                _downY = e.RawY;
                _isTap = true;
            }
            else if (e.ActionMasked is MotionEventActions.Cancel or MotionEventActions.PointerDown
                || Math.Abs(e.RawX - _downX) > _touchSlop || Math.Abs(e.RawY - _downY) > _touchSlop)
            {
                _isTap = false;
            }

            var outsideTap = e.ActionMasked == MotionEventActions.Up && _isTap
                && e.EventTime - e.DownTime < ViewConfiguration.LongPressTimeout;
            if (outsideTap && handler.VirtualView is TapDismissScrollView scroll
                && scroll.TapExclusion?.Handler?.PlatformView is Android.Views.View excluded)
            {
                using var bounds = new Android.Graphics.Rect();
                outsideTap = !excluded.GetGlobalVisibleRect(bounds) || !bounds.Contains((int)e.RawX, (int)e.RawY);
            }

            // Keep delivery and consumption entirely under native ScrollView/child control.
            var handled = base.DispatchTouchEvent(e);
            if (e.ActionMasked == MotionEventActions.Up) _isTap = false;
            if (outsideTap && handler.VirtualView is TapDismissScrollView target) target.NotifyOutsideTap();
            return handled;
        }
    }
}
