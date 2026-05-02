namespace Task_Roster.Views.EmployeeDashboardTabs;

public partial class EmployeeNotification : ContentPage
{
    public EmployeeNotification()
    {
        InitializeComponent();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}