namespace Microsoft.Maui.Storage
{
    // Exercise the real SQLite repository with bundled files and an isolated database.
    internal static class FileSystem
    {
        public static string AppDataDirectory { get; } = Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), "ImproveYourself.Checks", Guid.NewGuid().ToString("N"))).FullName;

        public static Task<Stream> OpenAppPackageFileAsync(string name) => Task.FromResult<Stream>(
            File.OpenRead(Path.Combine(AppContext.BaseDirectory,
                name.Replace("daily-challenges.", "course.").Replace("strings/", "strings."))));
    }
}

namespace ConfidenceCourse.Checks
{
    internal sealed class TestLocalization : ImproveYourself.Maui.Application.ILocalizationService
    {
        public string CurrentLanguage { get; private set; } = "en";
        public event EventHandler? LanguageChanged;
        public void Initialize() { }
        public void SetLanguage(string languageCode)
        {
            CurrentLanguage = languageCode;
            LanguageChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
