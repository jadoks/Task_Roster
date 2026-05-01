namespace Task_Roster.Views;

public partial class SignUpPage : ContentPage
{
    private string _selectedRole = string.Empty;

    public SignUpPage()
    {
        InitializeComponent();
    }

    private void OnTogglePasswordClicked(object sender, EventArgs e)
    {
        PasswordEntry.IsPassword = !PasswordEntry.IsPassword;

        PasswordToggleButton.Source = PasswordEntry.IsPassword
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
        if (string.IsNullOrWhiteSpace(EmailEntry.Text) ||
            string.IsNullOrWhiteSpace(FirstNameEntry.Text) ||
            string.IsNullOrWhiteSpace(LastNameEntry.Text) ||
            string.IsNullOrWhiteSpace(PasswordEntry.Text) ||
            string.IsNullOrWhiteSpace(_selectedRole))
        {
            await DisplayAlert("Missing Information", "Please complete all fields and select a role.", "OK");
            return;
        }

        Preferences.Set("IsLoggedIn", true);
        Preferences.Set("UserRole", _selectedRole);
        Preferences.Set("UserName", FirstNameEntry.Text.Trim());

        Application.Current!.Windows[0].Page = new AppShell();

        await Task.Delay(100);

        await Shell.Current.GoToAsync(nameof(DashboardPage));
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}