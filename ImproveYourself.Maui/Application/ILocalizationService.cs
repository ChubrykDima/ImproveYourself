namespace ImproveYourself.Maui.Application;

public interface ILocalizationService
{
    string CurrentLanguage { get; }

    event EventHandler? LanguageChanged;

    void Initialize();

    void SetLanguage(string languageCode);
}
