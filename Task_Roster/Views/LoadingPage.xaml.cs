namespace Task_Roster.Views;

public partial class LoadingPage : ContentPage
{
    public LoadingPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadingLine.ScaleTo(1.6, 500, Easing.CubicInOut);
        await LoadingLine.ScaleTo(1.0, 500, Easing.CubicInOut);

        await Task.Delay(700);

        bool isLoggedIn = Preferences.Get("IsLoggedIn", false);

        if (isLoggedIn)
            await Shell.Current.GoToAsync(nameof(DashboardPage));
        else
            await Shell.Current.GoToAsync(nameof(SignInPage));
    }
}