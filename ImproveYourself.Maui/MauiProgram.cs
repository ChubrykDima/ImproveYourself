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
		builder.UseMauiApp<App>();
#if ANDROID
        builder.ConfigureMauiHandlers(handlers =>
            handlers.AddHandler<Views.TapDismissScrollView, TapDismissScrollViewHandler>());
#endif

		var sentryDsn = SentryDsnResolver.ResolveDsn();
		if (!string.IsNullOrWhiteSpace(sentryDsn))
		{
			try
			{
				builder.UseSentry(options =>
				{
					options.Dsn = sentryDsn;
					options.SendDefaultPii = false;
					options.TracesSampleRate = 0;
					var release = SentryDsnResolver.ResolveRelease();
					if (!string.IsNullOrWhiteSpace(release))
					{
						options.Release = release;
					}

#if DEBUG
					options.Debug = true;
					options.Environment = "development";
#else
					options.Environment = "production";
#endif
				});
			}
			catch (Exception ex)
			{
				System.Diagnostics.Trace.WriteLine($"Sentry initialization failed: {ex}");
			}
		}

		builder.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddSingleton<IChallengeRepository, SqliteChallengeRepository>();
		builder.Services.AddSingleton<ISettingsService, PreferencesSettingsService>();
		builder.Services.AddSingleton<IAuthTokenStore, SecureAuthTokenStore>();
		builder.Services.AddSingleton<InstallationSessionGuard>();
		builder.Services.AddSingleton<ILocalizationService, LocalizationService>();
		builder.Services.AddSingleton<INotificationPreferenceService, NotificationPreferenceService>();
		builder.Services.AddSingleton<IAuthService>(sp =>
		{
			var settingsService = sp.GetRequiredService<ISettingsService>();
			var tokenStore = sp.GetRequiredService<IAuthTokenStore>();
			var authHttpClient = new HttpClient
			{
				Timeout = TimeSpan.FromSeconds(12),
			};

			return new AuthService(authHttpClient, settingsService, tokenStore);
		});
		builder.Services.AddTransient<AuthenticatedHttpMessageHandler>();
		builder.Services.AddSingleton(sp =>
		{
			var handler = sp.GetRequiredService<AuthenticatedHttpMessageHandler>();
			handler.InnerHandler = new HttpClientHandler();

			return new HttpClient(handler)
			{
				Timeout = TimeSpan.FromSeconds(12),
			};
		});
		builder.Services.AddSingleton<IAnalyticsClient, AnalyticsClient>();
		builder.Services.AddSingleton<BackendConnectionService>();
		builder.Services.AddSingleton<IBackendConnectionService>(services => services.GetRequiredService<BackendConnectionService>());
		builder.Services.AddSingleton<IBackendSyncService>(services => services.GetRequiredService<BackendConnectionService>());
		builder.Services.AddSingleton<AppState>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
