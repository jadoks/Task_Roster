namespace Task_Roster.Views.EmployeeDashboardTabs;

public partial class EmployeeDashboardPage : ContentPage
{
    public EmployeeDashboardPage()
    {
        InitializeComponent();
        MainContent.Content = new EmployeeHomeView();
    }

    private void OnTabChanged(object sender, int index)
    {
        MainContent.Content = index switch
        {
            0 => new EmployeeHomeView(),
            1 => new EmployeeSettingsView(),
            _ => new EmployeeHomeView()
        };
    }
}