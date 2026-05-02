using Task_Roster.Models;
using Task_Roster.Services;
using Task_Roster.Views.EmployeeDashboardTabs;

namespace Task_Roster.Views;

public partial class SignInPage : ContentPage
{
    private readonly DatabaseService _databaseService;

    private string _generatedPin = "";
    private string _resetEmail = "";

    public SignInPage()
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

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string email = EmailEntry.Text?.Trim().ToLower() ?? "";
        string password = PasswordEntry.Text ?? "";

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Missing Information", "Please enter your email and password.", "OK");
            return;
        }

        UserModel? user =
            await _databaseService.LoginUserAsync(email, password);

        if (user == null)
        {
            await DisplayAlert("Invalid Login", "Incorrect email or password.", "OK");
            return;
        }

        Preferences.Set("IsLoggedIn", true);
        Preferences.Set("UserRole", user.Role);
        Preferences.Set("UserName", user.FirstName);
        Preferences.Set("UserFirstName", user.FirstName);
        Preferences.Set("UserLastName", user.LastName);
        Preferences.Set("UserEmail", user.Email);

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
        else
        {
            Preferences.Clear();
            await DisplayAlert("Login Error", "Your account role is not supported.", "OK");
        }
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SignUpPage());
    }

    private void OnForgotPasswordTapped(object sender, TappedEventArgs e)
    {
        ResetForgotPasswordFlow();
        ForgotPasswordOverlay.IsVisible = true;
    }

    private void OnCloseForgotPasswordClicked(object sender, EventArgs e)
    {
        ForgotPasswordOverlay.IsVisible = false;
        ResetForgotPasswordFlow();
    }

    private async void OnProceedEmailClicked(object sender, EventArgs e)
    {
        string email = ResetEmailEntry.Text?.Trim().ToLower() ?? "";

        if (string.IsNullOrWhiteSpace(email))
        {
            await DisplayAlert("Missing Email", "Please enter your email address.", "OK");
            return;
        }

        UserModel? user =
            await _databaseService.GetUserByEmailAsync(email);

        if (user == null)
        {
            await DisplayAlert("Email Not Found", "No account found using this email.", "OK");
            return;
        }

        _resetEmail = email;

        Random random = new();
        _generatedPin = random.Next(1000, 9999).ToString();

        await DisplayAlert("Verification PIN", $"Your PIN is: {_generatedPin}", "OK");

        ForgotModalTitle.Text = "Verification PIN";
        ForgotEmailStep.IsVisible = false;
        ForgotPinStep.IsVisible = true;
        ForgotNewPasswordStep.IsVisible = false;
    }

    private async void OnProceedPinClicked(object sender, EventArgs e)
    {
        string enteredPin =
            $"{Pin1Entry.Text}" +
            $"{Pin2Entry.Text}" +
            $"{Pin3Entry.Text}" +
            $"{Pin4Entry.Text}";

        if (enteredPin.Length != 4)
        {
            await DisplayAlert("Missing PIN", "Please enter the 4-digit PIN.", "OK");
            return;
        }

        if (enteredPin != _generatedPin)
        {
            await DisplayAlert("Invalid PIN", "The PIN you entered is incorrect.", "OK");
            return;
        }

        ForgotModalTitle.Text = "Set New Password";
        ForgotEmailStep.IsVisible = false;
        ForgotPinStep.IsVisible = false;
        ForgotNewPasswordStep.IsVisible = true;
    }

    private void OnToggleNewPasswordClicked(object sender, EventArgs e)
    {
        NewPasswordEntry.IsPassword = !NewPasswordEntry.IsPassword;

        NewPasswordToggleButton.Source =
            NewPasswordEntry.IsPassword
            ? "eyeslash.svg"
            : "eye.svg";
    }

    private void OnToggleConfirmPasswordClicked(object sender, EventArgs e)
    {
        ConfirmPasswordEntry.IsPassword = !ConfirmPasswordEntry.IsPassword;

        ConfirmPasswordToggleButton.Source =
            ConfirmPasswordEntry.IsPassword
            ? "eyeslash.svg"
            : "eye.svg";
    }

    private async void OnResetPasswordClicked(object sender, EventArgs e)
    {
        string newPassword = NewPasswordEntry.Text?.Trim() ?? "";
        string confirmPassword = ConfirmPasswordEntry.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(newPassword) ||
            string.IsNullOrWhiteSpace(confirmPassword))
        {
            await DisplayAlert("Missing Password", "Please enter and confirm your new password.", "OK");
            return;
        }

        if (newPassword != confirmPassword)
        {
            await DisplayAlert("Password Mismatch", "Passwords do not match.", "OK");
            return;
        }

        await _databaseService.UpdatePasswordAsync(_resetEmail, newPassword);

        await DisplayAlert("Success", "Your password has been reset successfully.", "OK");

        ForgotPasswordOverlay.IsVisible = false;
        ResetForgotPasswordFlow();
    }

    private void ResetForgotPasswordFlow()
    {
        ForgotModalTitle.Text = "Forgot Password";

        ForgotEmailStep.IsVisible = true;
        ForgotPinStep.IsVisible = false;
        ForgotNewPasswordStep.IsVisible = false;

        ResetEmailEntry.Text = "";

        Pin1Entry.Text = "";
        Pin2Entry.Text = "";
        Pin3Entry.Text = "";
        Pin4Entry.Text = "";

        NewPasswordEntry.Text = "";
        ConfirmPasswordEntry.Text = "";

        NewPasswordEntry.IsPassword = true;
        ConfirmPasswordEntry.IsPassword = true;

        NewPasswordToggleButton.Source = "eyeslash.svg";
        ConfirmPasswordToggleButton.Source = "eyeslash.svg";

        _generatedPin = "";
        _resetEmail = "";
    }
}