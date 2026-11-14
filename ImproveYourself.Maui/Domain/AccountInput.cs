using System.Globalization;
using System.Text.RegularExpressions;
using ImproveYourself.Maui.Resources.Strings;

namespace ImproveYourself.Maui.Domain;

public static class AccountInput
{
    public const int DisplayNameMaxLength = 24;

    private static readonly Regex EmailPattern = new(
        "^[A-Za-z0-9._%+\\-]+@[A-Za-z0-9.\\-]+\\.[A-Za-z]{2,}$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public static string FirstGrapheme(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var enumerator = StringInfo.GetTextElementEnumerator(value.Trim());

        return enumerator.MoveNext() ? enumerator.GetTextElement() : string.Empty;
    }

    public static string? ValidateDisplayName(string? name)
    {
        var trimmed = name?.Trim() ?? string.Empty;

        if (trimmed.Length == 0)
        {
            return AppStrings.NameRequired;
        }

        if (trimmed.Length > DisplayNameMaxLength)
        {
            return AppStrings.NameLengthHint;
        }

        return null;
    }

    public static string? ValidateRegistration(string? email, string? password)
    {
        var normalizedEmail = email?.Trim() ?? string.Empty;
        var normalizedPassword = password ?? string.Empty;

        if (string.IsNullOrEmpty(normalizedEmail))
        {
            return AppStrings.AuthEmailRequired;
        }

        if (!EmailPattern.IsMatch(normalizedEmail))
        {
            return AppStrings.AuthEmailInvalid;
        }

        if (normalizedPassword.Length < 8)
        {
            return AppStrings.AuthPasswordTooShort;
        }

        return null;
    }

    public static string? ValidateLogin(string? email, string? password)
    {
        if (string.IsNullOrWhiteSpace(email)) return AppStrings.AuthEmailRequired;
        if (!EmailPattern.IsMatch(email.Trim())) return AppStrings.AuthEmailInvalid;
        return string.IsNullOrEmpty(password) ? AppStrings.AuthPasswordRequired : null;
    }
}
