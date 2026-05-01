using Task_Roster.Views.EmployeeDashboardTabs;

namespace Task_Roster.Views;

public partial class SignInPage : ContentPage
{
    public SignInPage()
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

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string email = EmailEntry.Text?.Trim().ToLower() ?? string.Empty;
        string password = PasswordEntry.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Missing Information", "Please enter your email and password.", "OK");
            return;
        }

        if (email == "manager@demo.com" && password == "123456")
        {
            Preferences.Set("IsLoggedIn", true);
            Preferences.Set("UserRole", "Manager");

            Application.Current!.Windows[0].Page = new AppShell();

            await Task.Delay(100);
            await Shell.Current.GoToAsync(nameof(DashboardPage));
            return;
        }

        if (email == "employee@demo.com" && password == "123456")
        {
            Preferences.Set("IsLoggedIn", true);
            Preferences.Set("UserRole", "Employee");

            Application.Current!.Windows[0].Page = new EmployeeDashboardPage();
            return;
        }

        await DisplayAlert("Invalid Login", "Please use a valid demo account.", "OK");
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
        if (string.IsNullOrWhiteSpace(ResetEmailEntry.Text))
        {
            await DisplayAlert("Missing Email", "Please enter your email address.", "OK");
            return;
        }

        ForgotModalTitle.Text = "Verification PIN";
        ForgotEmailStep.IsVisible = false;
        ForgotPinStep.IsVisible = true;
        ForgotNewPasswordStep.IsVisible = false;
    }

    private async void OnProceedPinClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Pin1Entry.Text) ||
            string.IsNullOrWhiteSpace(Pin2Entry.Text) ||
            string.IsNullOrWhiteSpace(Pin3Entry.Text) ||
            string.IsNullOrWhiteSpace(Pin4Entry.Text))
        {
            await DisplayAlert("Missing PIN", "Please enter the 4-digit PIN.", "OK");
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

        NewPasswordToggleButton.Source = NewPasswordEntry.IsPassword
            ? "eyeslash.svg"
            : "eye.svg";
    }

    private void OnToggleConfirmPasswordClicked(object sender, EventArgs e)
    {
        ConfirmPasswordEntry.IsPassword = !ConfirmPasswordEntry.IsPassword;

        ConfirmPasswordToggleButton.Source = ConfirmPasswordEntry.IsPassword
            ? "eyeslash.svg"
            : "eye.svg";
    }

    private async void OnResetPasswordClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NewPasswordEntry.Text) ||
            string.IsNullOrWhiteSpace(ConfirmPasswordEntry.Text))
        {
            await DisplayAlert("Missing Password", "Please enter and confirm your new password.", "OK");
            return;
        }

        if (NewPasswordEntry.Text != ConfirmPasswordEntry.Text)
        {
            await DisplayAlert("Password Mismatch", "New password and confirmation password do not match.", "OK");
            return;
        }

        await DisplayAlert("Password Reset", "Your password has been reset.", "OK");

        ForgotPasswordOverlay.IsVisible = false;
        ResetForgotPasswordFlow();
    }

    private void ResetForgotPasswordFlow()
    {
        ForgotModalTitle.Text = "Forgot Password";

        ForgotEmailStep.IsVisible = true;
        ForgotPinStep.IsVisible = false;
        ForgotNewPasswordStep.IsVisible = false;

        ResetEmailEntry.Text = string.Empty;

        Pin1Entry.Text = string.Empty;
        Pin2Entry.Text = string.Empty;
        Pin3Entry.Text = string.Empty;
        Pin4Entry.Text = string.Empty;

        NewPasswordEntry.Text = string.Empty;
        ConfirmPasswordEntry.Text = string.Empty;

        NewPasswordEntry.IsPassword = true;
        ConfirmPasswordEntry.IsPassword = true;

        NewPasswordToggleButton.Source = "eyeslash.svg";
        ConfirmPasswordToggleButton.Source = "eyeslash.svg";
    }
}