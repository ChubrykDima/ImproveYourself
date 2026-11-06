using System.Runtime.CompilerServices;

namespace ImproveYourself.Maui.Views;

internal static class PasswordField
{
    private static readonly ConditionalWeakTable<Entry, object> Configured = new();

    public static void Configure(Entry entry)
    {
        entry.IsSpellCheckEnabled = false;
        entry.IsTextPredictionEnabled = false;

        if (Configured.TryGetValue(entry, out _))
        {
            ApplyPlatform(entry);
            return;
        }

        Configured.Add(entry, new object());
        entry.HandlerChanged += (_, _) => ApplyPlatform(entry);
        ApplyPlatform(entry);
    }

    private static void ApplyPlatform(Entry entry)
    {
#if IOS || MACCATALYST
        if (entry.Handler?.PlatformView is UIKit.UITextField field)
        {
            // OneTimeCode suppresses the iOS "Use Strong Password?" sheet, which clears
            // the field when the sheet is dismissed.
            field.TextContentType = UIKit.UITextContentType.OneTimeCode;
            field.AutocorrectionType = UIKit.UITextAutocorrectionType.No;
            field.SpellCheckingType = UIKit.UITextSpellCheckingType.No;
        }
#endif
    }
}
