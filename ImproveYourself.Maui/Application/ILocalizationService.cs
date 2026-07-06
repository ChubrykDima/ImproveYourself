namespace ImproveYourself.Maui.Application;

public interface ILocalizationService
{
    string CurrentLanguage { get; }

    void Initialize();

    void SetLanguage(string languageCode);
}
