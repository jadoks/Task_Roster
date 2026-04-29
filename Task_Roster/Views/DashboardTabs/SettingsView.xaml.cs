namespace Task_Roster.Views.DashboardTabs;

public partial class SettingsView : ContentView
{
    public SettingsView()
    {
        InitializeComponent();
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        Preferences.Set("IsLoggedIn", false);
        await Shell.Current.GoToAsync(nameof(SignInPage));
    }
}