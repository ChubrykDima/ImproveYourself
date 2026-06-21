using ImproveYourself.Maui.Application;
using ImproveYourself.Maui.Domain;
using ImproveYourself.Maui.Theme;
using ImproveYourself.Maui.Views;
using Microsoft.Maui.ApplicationModel;

namespace ImproveYourself.Maui;

public partial class App : Microsoft.Maui.Controls.Application
{
	private readonly AppState _appState;
	private Page _rootPage;
	private Window? _window;

	public App(AppState appState)
	{
		InitializeComponent();
		_appState = appState;
		_rootPage = BuildNavigationPage(new LoadingPage());

		_ = BootstrapAsync();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		_window = new Window(_rootPage);

		return _window;
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

					SetRootPage(BuildNavigationPage(new HomePage(_appState)));
					return;
				}

				SetRootPage(BuildNavigationPage(new OnboardingSlideOnePage(_appState, NavigateToHomeAsync)));
			});
		}
		catch
		{
			await MainThread.InvokeOnMainThreadAsync(() =>
			{
				SetRootPage(BuildNavigationPage(new LoadingPage("Не удалось инициализировать приложение.")));
			});
		}
	}

	private Task NavigateToHomeAsync()
	{
		SetRootPage(BuildNavigationPage(new HomePage(_appState)));

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
