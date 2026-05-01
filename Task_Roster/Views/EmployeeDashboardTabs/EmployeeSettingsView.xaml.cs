namespace Task_Roster.Views.EmployeeDashboardTabs;

public partial class EmployeeSettingsView : ContentView
{
    public EmployeeSettingsView()
    {
        InitializeComponent();
    }

    private void OnLogoutClicked(object sender, EventArgs e)
    {
        Preferences.Clear();
        Application.Current!.Windows[0].Page = new NavigationPage(new SignInPage());
    }
}