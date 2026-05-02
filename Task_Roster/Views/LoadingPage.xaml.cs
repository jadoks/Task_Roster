using Task_Roster.Views.EmployeeDashboardTabs;

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
        string userRole = Preferences.Get("UserRole", "");

        if (!isLoggedIn)
        {
            await Shell.Current.GoToAsync(nameof(SignInPage));
            return;
        }

        if (userRole == "Manager")
        {
            await Shell.Current.GoToAsync(nameof(DashboardPage));
        }
        else if (userRole == "Employee")
        {
            Application.Current!.Windows[0].Page =
                new EmployeeDashboardPage();
        }
        else
        {
            Preferences.Clear();
            await Shell.Current.GoToAsync(nameof(SignInPage));
        }
    }
}