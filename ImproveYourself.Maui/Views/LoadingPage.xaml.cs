using ImproveYourself.Maui.Resources.Strings;

namespace ImproveYourself.Maui.Views;

public partial class LoadingPage : ContentPage
{
    public LoadingPage(string? message = null)
    {
        InitializeComponent();
        MessageLabel.Text = message ?? AppStrings.AppPreparing;
    }
}
