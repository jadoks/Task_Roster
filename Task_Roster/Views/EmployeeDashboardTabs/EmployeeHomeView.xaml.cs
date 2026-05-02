using Task_Roster.Models;
using Task_Roster.Services;

namespace Task_Roster.Views.EmployeeDashboardTabs;

public partial class EmployeeHomeView : ContentView
{
    private readonly DatabaseService _databaseService;

    public EmployeeHomeView()
    {
        InitializeComponent();

        _databaseService = new DatabaseService();

        LoadCurrentUser();
    }

    private async void LoadCurrentUser()
    {
        string email = Preferences.Get("UserEmail", "");

        if (string.IsNullOrWhiteSpace(email))
        {
            GreetingLabel.Text = "Hi, Employee";
            DateLabel.Text = DateTime.Now.ToString("ddd, MMM dd");
            return;
        }

        UserModel? user = await _databaseService.GetUserByEmailAsync(email);

        if (user == null)
        {
            GreetingLabel.Text = "Hi, Employee";
            DateLabel.Text = DateTime.Now.ToString("ddd, MMM dd");
            return;
        }

        GreetingLabel.Text = $"Hi, {user.FirstName}";
        DateLabel.Text = DateTime.Now.ToString("ddd, MMM dd");
    }
}