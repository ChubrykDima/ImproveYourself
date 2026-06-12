namespace ImproveYourself.Maui.Views;

public partial class LoadingPage : ContentPage
{
    public LoadingPage(string message = "Подготовка приложения...")
    {
        InitializeComponent();
        MessageLabel.Text = message;
    }
}
