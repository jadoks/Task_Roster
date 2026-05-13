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

        // 1. Set the initial state of the line to be very small
        LoadingLine.ScaleX = 0.1;

        // 2. Scale the line horizontally from small to long over exactly 3 seconds
        // This replaces the old ScaleTo and Task.Delay logic
        await LoadingLine.ScaleXTo(4.0, 2000, Easing.CubicInOut);

        // 3. Original Navigation Logic (Unchanged)
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