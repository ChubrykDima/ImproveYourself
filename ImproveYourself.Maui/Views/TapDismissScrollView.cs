namespace ImproveYourself.Maui.Views;

// Observes outside taps without installing an Android gesture recognizer on the
// scroll container: that recognizer prevents native vertical scrolling.
public sealed class TapDismissScrollView : ScrollView
{
    public VisualElement? TapExclusion { get; set; }
    public event EventHandler? OutsideTapped;

    public TapDismissScrollView()
    {
#if !ANDROID
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, args) =>
        {
            if (TapExclusion is { } excluded && args.GetPosition(excluded) is Point point
                && point.X >= 0 && point.Y >= 0 && point.X <= excluded.Width && point.Y <= excluded.Height)
            {
                return;
            }
            NotifyOutsideTap();
        };
        GestureRecognizers.Add(tap);
#endif
    }

    internal void NotifyOutsideTap() => OutsideTapped?.Invoke(this, EventArgs.Empty);
}
