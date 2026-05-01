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

    // =========================
    // PASSWORD TOGGLE
    // =========================

    private void OnTogglePasswordClicked(
        object sender,
        EventArgs e)
    {
        PasswordEntry.IsPassword =
            !PasswordEntry.IsPassword;

        PasswordToggleButton.Source =
            PasswordEntry.IsPassword
            ? "eyeslash.svg"
            : "eye.svg";
    }

    // =========================
    // LOGIN
    // =========================

    private async void OnLoginClicked(
        object sender,
        EventArgs e)
    {
        string email =
            EmailEntry.Text?.Trim().ToLower()
            ?? "";

        string password =
            PasswordEntry.Text ?? "";

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert(
                "Missing Information",
                "Please enter your email and password.",
                "OK");

            return;
        }

        // =========================
        // DEMO MANAGER
        // =========================

        if (email == "manager@demo.com"
            && password == "123456")
        {
            Preferences.Set("IsLoggedIn", true);

            Preferences.Set("UserRole", "Manager");

            Preferences.Set("UserName", "Manager");

            Preferences.Set(
                "UserEmail",
                "manager@demo.com");

            Application.Current!
                .Windows[0]
                .Page = new AppShell();

            await Task.Delay(100);

            await Shell.Current.GoToAsync(
                nameof(DashboardPage));

            return;
        }

        // =========================
        // DEMO EMPLOYEE
        // =========================

        if (email == "employee@demo.com"
            && password == "123456")
        {
            Preferences.Set("IsLoggedIn", true);

            Preferences.Set("UserRole", "Employee");

            Preferences.Set("UserName", "Employee");

            Preferences.Set(
                "UserEmail",
                "employee@demo.com");

            Application.Current!
                .Windows[0]
                .Page =
                    new EmployeeDashboardPage();

            return;
        }

        // =========================
        // SQLITE LOGIN
        // =========================

        UserModel? user =
            await _databaseService
                .LoginUserAsync(
                    email,
                    password);

        if (user == null)
        {
            await DisplayAlert(
                "Invalid Login",
                "Incorrect email or password.",
                "OK");

            return;
        }

        Preferences.Set("IsLoggedIn", true);

        Preferences.Set("UserRole", user.Role);

        Preferences.Set(
            "UserName",
            user.FirstName);

        Preferences.Set(
            "UserEmail",
            user.Email);

        // =========================
        // ROLE NAVIGATION
        // =========================

        if (user.Role == "Manager")
        {
            Application.Current!
                .Windows[0]
                .Page = new AppShell();

            await Task.Delay(100);

            await Shell.Current.GoToAsync(
                nameof(DashboardPage));
        }
        else
        {
            Application.Current!
                .Windows[0]
                .Page =
                    new EmployeeDashboardPage();
        }
    }

    // =========================
    // REGISTER
    // =========================

    private async void OnRegisterClicked(
        object sender,
        EventArgs e)
    {
        await Navigation.PushAsync(
            new SignUpPage());
    }

    // =========================
    // FORGOT PASSWORD OPEN
    // =========================

    private void OnForgotPasswordTapped(
        object sender,
        TappedEventArgs e)
    {
        ResetForgotPasswordFlow();

        ForgotPasswordOverlay.IsVisible = true;
    }

    // =========================
    // CLOSE MODAL
    // =========================

    private void OnCloseForgotPasswordClicked(
        object sender,
        EventArgs e)
    {
        ForgotPasswordOverlay.IsVisible = false;

        ResetForgotPasswordFlow();
    }

    // =========================
    // EMAIL STEP
    // =========================

    private async void OnProceedEmailClicked(
        object sender,
        EventArgs e)
    {
        string email =
            ResetEmailEntry.Text?.Trim()
            ?? "";

        if (string.IsNullOrWhiteSpace(email))
        {
            await DisplayAlert(
                "Missing Email",
                "Please enter your email address.",
                "OK");

            return;
        }

        var user =
            await _databaseService
                .GetUserByEmailAsync(email);

        if (user == null)
        {
            await DisplayAlert(
                "Email Not Found",
                "No account found using this email.",
                "OK");

            return;
        }

        _resetEmail = email;

        // GENERATE RANDOM PIN
        Random random = new();

        _generatedPin =
            random.Next(1000, 9999).ToString();

        // SIMULATED EMAIL
        await DisplayAlert(
            "Verification PIN",
            $"Your PIN is: {_generatedPin}",
            "OK");

        ForgotModalTitle.Text =
            "Verification PIN";

        ForgotEmailStep.IsVisible = false;

        ForgotPinStep.IsVisible = true;

        ForgotNewPasswordStep.IsVisible = false;
    }

    // =========================
    // PIN STEP
    // =========================

    private async void OnProceedPinClicked(
        object sender,
        EventArgs e)
    {
        string enteredPin =
            $"{Pin1Entry.Text}" +
            $"{Pin2Entry.Text}" +
            $"{Pin3Entry.Text}" +
            $"{Pin4Entry.Text}";

        if (enteredPin.Length != 4)
        {
            await DisplayAlert(
                "Missing PIN",
                "Please enter the 4-digit PIN.",
                "OK");

            return;
        }

        if (enteredPin != _generatedPin)
        {
            await DisplayAlert(
                "Invalid PIN",
                "The PIN you entered is incorrect.",
                "OK");

            return;
        }

        ForgotModalTitle.Text =
            "Set New Password";

        ForgotEmailStep.IsVisible = false;

        ForgotPinStep.IsVisible = false;

        ForgotNewPasswordStep.IsVisible = true;
    }

    // =========================
    // TOGGLE NEW PASSWORD
    // =========================

    private void OnToggleNewPasswordClicked(
        object sender,
        EventArgs e)
    {
        NewPasswordEntry.IsPassword =
            !NewPasswordEntry.IsPassword;

        NewPasswordToggleButton.Source =
            NewPasswordEntry.IsPassword
            ? "eyeslash.svg"
            : "eye.svg";
    }

    // =========================
    // TOGGLE CONFIRM PASSWORD
    // =========================

    private void OnToggleConfirmPasswordClicked(
        object sender,
        EventArgs e)
    {
        ConfirmPasswordEntry.IsPassword =
            !ConfirmPasswordEntry.IsPassword;

        ConfirmPasswordToggleButton.Source =
            ConfirmPasswordEntry.IsPassword
            ? "eyeslash.svg"
            : "eye.svg";
    }

    // =========================
    // RESET PASSWORD
    // =========================

    private async void OnResetPasswordClicked(
        object sender,
        EventArgs e)
    {
        string newPassword =
            NewPasswordEntry.Text?.Trim()
            ?? "";

        string confirmPassword =
            ConfirmPasswordEntry.Text?.Trim()
            ?? "";

        if (string.IsNullOrWhiteSpace(newPassword) ||
            string.IsNullOrWhiteSpace(confirmPassword))
        {
            await DisplayAlert(
                "Missing Password",
                "Please enter and confirm your new password.",
                "OK");

            return;
        }

        if (newPassword != confirmPassword)
        {
            await DisplayAlert(
                "Password Mismatch",
                "Passwords do not match.",
                "OK");

            return;
        }

        await _databaseService.UpdatePasswordAsync(
            _resetEmail,
            newPassword);

        await DisplayAlert(
            "Success",
            "Your password has been reset successfully.",
            "OK");

        ForgotPasswordOverlay.IsVisible = false;

        ResetForgotPasswordFlow();
    }

    // =========================
    // RESET FLOW
    // =========================

    private void ResetForgotPasswordFlow()
    {
        ForgotModalTitle.Text =
            "Forgot Password";

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

        NewPasswordToggleButton.Source =
            "eyeslash.svg";

        ConfirmPasswordToggleButton.Source =
            "eyeslash.svg";

        _generatedPin = "";

        _resetEmail = "";
    }
}