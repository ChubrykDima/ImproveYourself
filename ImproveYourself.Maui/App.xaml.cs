using ImproveYourself.Maui.Application;
using ImproveYourself.Maui.Domain;
using ImproveYourself.Maui.Resources.Strings;
using ImproveYourself.Maui.Theme;
using ImproveYourself.Maui.Views;
using Microsoft.Maui.ApplicationModel;

namespace ImproveYourself.Maui;

public partial class App : Microsoft.Maui.Controls.Application
{
	private readonly AppState _appState;
	private readonly IBackendConnectionService _backendConnectionService;
	private readonly ILocalizationService _localizationService;
	private Page _rootPage;
	private Window? _window;

	public App(AppState appState, IBackendConnectionService backendConnectionService, ILocalizationService localizationService)
	{
		// Initialize localization before any UI is constructed so all pages
		// receive the correct strings during InitializeComponent().
		localizationService.Initialize();

		InitializeComponent();
		_appState = appState;
		_backendConnectionService = backendConnectionService;
		_localizationService = localizationService;
		_rootPage = BuildNavigationPage(new LoadingPage());

		_ = BootstrapAsync();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		_window = new Window(_rootPage);

		return _window;
	}

	/// <summary>
	/// Reloads the navigation root after a language change so all pages are
	/// re-created with the newly applied culture.
	/// </summary>
	public void ReloadNavigation()
	{
		_appState.ReloadAfterLanguageChange();

		if (_appState.OnboardingCompleted)
		{
			SetRootPage(BuildNavigationPage(new HomePage(_appState, _backendConnectionService, _localizationService)));
		}
		else
		{
			SetRootPage(BuildNavigationPage(new OnboardingSlideOnePage(_appState, NavigateToHomeAsync)));
		}
	}

	private static NavigationPage BuildNavigationPage(Page rootPage)
	{
		return new NavigationPage(rootPage)
		{
			BarBackgroundColor = ThemeTokens.SurfaceColor,
			BarTextColor = ThemeTokens.TextPrimaryColor,
			BackgroundColor = ThemeTokens.BackgroundColor,
		};
	}

	private async Task BootstrapAsync()
	{
		try
		{
			await _appState.InitializeAsync();

			await MainThread.InvokeOnMainThreadAsync(() =>
			{
				if (_appState.OnboardingCompleted)
				{
					if (_appState.ShouldShowStartSelfAssessment)
					{
						SetRootPage(BuildNavigationPage(new SelfAssessmentPage(
							_appState,
							SelfAssessmentKind.Start,
							NavigateToHomeAsync)));
						return;
					}

				SetRootPage(BuildNavigationPage(new HomePage(_appState, _backendConnectionService, _localizationService)));
				return;
			}

			SetRootPage(BuildNavigationPage(new OnboardingSlideOnePage(_appState, NavigateToHomeAsync)));
			});
		}
		catch
		{
			await MainThread.InvokeOnMainThreadAsync(() =>
			{
				SetRootPage(BuildNavigationPage(new LoadingPage(AppStrings.AppInitError)));
			});
		}
	}

	private Task NavigateToHomeAsync()
	{
		SetRootPage(BuildNavigationPage(new HomePage(_appState, _backendConnectionService, _localizationService)));

		return Task.CompletedTask;
	}

	private void SetRootPage(Page page)
	{
		_rootPage = page;

		if (_window is not null)
		{
			_window.Page = page;
		}
	}
}
