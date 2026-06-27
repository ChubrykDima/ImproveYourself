using Microsoft.Extensions.Logging;
using ImproveYourself.Maui.Application;
using ImproveYourself.Maui.Persistence;

namespace ImproveYourself.Maui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		SQLitePCL.Batteries_V2.Init();

		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddSingleton<IChallengeRepository, SqliteChallengeRepository>();
		builder.Services.AddSingleton<ISettingsService, PreferencesSettingsService>();
		builder.Services.AddSingleton<INotificationPreferenceService, NotificationPreferenceService>();
		builder.Services.AddSingleton(new HttpClient
		{
			Timeout = TimeSpan.FromSeconds(12),
		});
		builder.Services.AddSingleton<IBackendConnectionService, BackendConnectionService>();
		builder.Services.AddSingleton<AppState>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
