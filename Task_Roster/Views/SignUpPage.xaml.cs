using Task_Roster.Models;
using Task_Roster.Services;
using Task_Roster.Views.EmployeeDashboardTabs;

namespace Task_Roster.Views;

public partial class SignUpPage : ContentPage
{
    private string _selectedRole = string.Empty;

    private readonly DatabaseService _databaseService;

    public SignUpPage()
    {
        InitializeComponent();
        _databaseService = new DatabaseService();
    }

    private void OnTogglePasswordClicked(object sender, EventArgs e)
    {
        PasswordEntry.IsPassword = !PasswordEntry.IsPassword;

        PasswordToggleButton.Source =
            PasswordEntry.IsPassword
            ? "eyeslash.svg"
            : "eye.svg";
    }

    private void OnManagerClicked(object sender, EventArgs e)
    {
        _selectedRole = "Manager";

        ManagerButton.BackgroundColor = Color.FromArgb("#165A3A");
        ManagerButton.TextColor = Colors.White;

        EmployeeButton.BackgroundColor = Color.FromArgb("#DCE5E0");
        EmployeeButton.TextColor = Color.FromArgb("#165A3A");
    }

    private void OnEmployeeClicked(object sender, EventArgs e)
    {
        _selectedRole = "Employee";

        EmployeeButton.BackgroundColor = Color.FromArgb("#165A3A");
        EmployeeButton.TextColor = Colors.White;

        ManagerButton.BackgroundColor = Color.FromArgb("#DCE5E0");
        ManagerButton.TextColor = Color.FromArgb("#165A3A");
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        string email = EmailEntry.Text?.Trim().ToLower() ?? "";
        string firstName = FirstNameEntry.Text?.Trim() ?? "";
        string lastName = LastNameEntry.Text?.Trim() ?? "";
        string password = PasswordEntry.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(firstName) ||
            string.IsNullOrWhiteSpace(lastName) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(_selectedRole))
        {
            await DisplayAlert(
                "Missing Information",
                "Please complete all fields and select a role.",
                "OK");

            return;
        }

        UserModel? existingUser =
            await _databaseService.GetUserByEmailAsync(email);

        if (existingUser != null)
        {
            await DisplayAlert(
                "Email Already Exists",
                "This email is already registered.",
                "OK");

            return;
        }

        var user = new UserModel
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Password = password,
            Role = _selectedRole
        };

        await _databaseService.AddUserAsync(user);

        Preferences.Set("IsLoggedIn", true);
        Preferences.Set("UserRole", user.Role);
        Preferences.Set("UserName", user.FirstName);
        Preferences.Set("UserFirstName", user.FirstName);
        Preferences.Set("UserLastName", user.LastName);
        Preferences.Set("UserEmail", user.Email);

        await DisplayAlert(
            "Success",
            "Account created successfully.",
            "OK");

        if (user.Role == "Manager")
        {
            Application.Current!.Windows[0].Page = new AppShell();

            await Task.Delay(100);
            await Shell.Current.GoToAsync(nameof(DashboardPage));
        }
        else if (user.Role == "Employee")
        {
            Application.Current!.Windows[0].Page =
                new EmployeeDashboardPage();
        }
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}