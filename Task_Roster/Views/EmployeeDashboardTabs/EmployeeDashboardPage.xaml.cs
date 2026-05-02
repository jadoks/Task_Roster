using Task_Roster.Models;
using Task_Roster.Services;

namespace Task_Roster.Views.EmployeeDashboardTabs;

public partial class EmployeeDashboardPage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private UserModel? _currentUser;

    public EmployeeDashboardPage()
    {
        InitializeComponent();

        _databaseService = new DatabaseService();

        LoadCurrentEmployee();
    }

    private async void LoadCurrentEmployee()
    {
        string email = Preferences.Get("UserEmail", "");

        if (!string.IsNullOrWhiteSpace(email))
        {
            _currentUser = await _databaseService.GetUserByEmailAsync(email);
        }

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