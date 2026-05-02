using Microsoft.Maui.Controls.Shapes;
using Task_Roster.Models;
using Task_Roster.Services;
using Task_Roster.Views;
using Microsoft.Maui.ApplicationModel.Communication;

namespace Task_Roster.Views.EmployeeDashboardTabs;

public partial class EmployeeSettingsView : ContentView
{
    private readonly DatabaseService _databaseService;


    private string _currentEmail = "";

    public EmployeeSettingsView()
    {
        InitializeComponent();

        _databaseService = new DatabaseService();
       

        LoadCurrentUser();
    }

    private async void LoadCurrentUser()
    {
        string email = Preferences.Get("UserEmail", "");

        if (string.IsNullOrWhiteSpace(email))
            return;

        UserModel? user =
            await _databaseService.GetUserByEmailAsync(email);

        if (user == null)
            return;

        _currentEmail = user.Email;

        string fullName =
            $"{user.FirstName} {user.LastName}".Trim();

        ProfileNameLabel.Text = fullName;
        ProfileEmailLabel.Text = user.Email;

        EditNameEntry.Text = fullName;
        EditEmailEntry.Text = user.Email;

        string initials =
            $"{GetInitial(user.FirstName)}{GetInitial(user.LastName)}";

        ProfileInitialsLabel.Text = initials;
        EditProfileInitialsLabel.Text = initials;

        if (user.ProfileImageBytes != null &&
            user.ProfileImageBytes.Length > 0)
        {
            ProfileImage.Source =
                ImageSource.FromStream(
                    () => new MemoryStream(user.ProfileImageBytes));

            EditProfileImage.Source =
                ImageSource.FromStream(
                    () => new MemoryStream(user.ProfileImageBytes));

            ProfileImage.IsVisible = true;
            EditProfileImage.IsVisible = true;

            ProfileInitialsLabel.IsVisible = false;
            EditProfileInitialsLabel.IsVisible = false;
        }
        else
        {
            ProfileImage.Source = null;
            EditProfileImage.Source = null;

            ProfileImage.IsVisible = false;
            EditProfileImage.IsVisible = false;

            ProfileInitialsLabel.IsVisible = true;
            EditProfileInitialsLabel.IsVisible = true;
        }
    }

    private void OnEditProfileClicked(object sender, EventArgs e)
    {
        SettingsMainView.IsVisible = false;
        EditProfileView.IsVisible = true;
    }

    private void OnBackToSettingsClicked(object sender, EventArgs e)
    {
        SettingsMainView.IsVisible = true;
        EditProfileView.IsVisible = false;
    }

    private async void OnBellClicked(object sender, TappedEventArgs e)
    {
        Page? page = Window?.Page;

        if (page != null)
            await page.Navigation.PushAsync(new EmployeeNotification());
    }

    private async void OnProfileCameraTapped(object sender, TappedEventArgs e)
    {
        try
        {
            FileResult? photo =
                await FilePicker.Default.PickAsync(
                    new PickOptions
                    {
                        PickerTitle = "Select Profile Picture",
                        FileTypes = FilePickerFileType.Images
                    });

            if (photo == null)
                return;

            using Stream sourceStream =
                await photo.OpenReadAsync();

            using MemoryStream memoryStream = new();

            await sourceStream.CopyToAsync(memoryStream);

            byte[] imageBytes = memoryStream.ToArray();

            UserModel? user =
                await _databaseService.GetUserByEmailAsync(_currentEmail);

            if (user == null)
            {
                await ShowAlert(
                    "Error",
                    "Current user was not found.");

                return;
            }

            user.ProfileImageBytes = imageBytes;

            await _databaseService.UpdateUserAsync(user);

            ProfileImage.Source =
                ImageSource.FromStream(
                    () => new MemoryStream(imageBytes));

            EditProfileImage.Source =
                ImageSource.FromStream(
                    () => new MemoryStream(imageBytes));

            ProfileImage.IsVisible = true;
            EditProfileImage.IsVisible = true;

            ProfileInitialsLabel.IsVisible = false;
            EditProfileInitialsLabel.IsVisible = false;
        }
        catch
        {
            await ShowAlert(
                "Error",
                "Unable to select profile picture.");
        }
    }

    private async void OnSaveProfileClicked(object sender, EventArgs e)
    {
        string fullName = EditNameEntry.Text?.Trim() ?? "";
        string newEmail = EditEmailEntry.Text?.Trim().ToLower() ?? "";

        if (string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(newEmail))
        {
            await ShowAlert(
                "Missing Information",
                "Please complete all fields.");

            return;
        }

        UserModel? currentUser =
            await _databaseService.GetUserByEmailAsync(_currentEmail);

        if (currentUser == null)
        {
            await ShowAlert(
                "Error",
                "Current user was not found.");

            return;
        }

        if (newEmail != _currentEmail.ToLower())
        {
            UserModel? existingUser =
                await _databaseService.GetUserByEmailAsync(newEmail);

            if (existingUser != null)
            {
                await ShowAlert(
                    "Email Already Exists",
                    "This email is already used by another account.");

                return;
            }
        }

        string[] nameParts = fullName.Split(' ', 2);

        currentUser.FirstName = nameParts[0];
        currentUser.LastName = nameParts.Length > 1 ? nameParts[1] : "";
        currentUser.Email = newEmail;

        await _databaseService.UpdateUserAsync(currentUser);

        Preferences.Set("UserEmail", currentUser.Email);
        Preferences.Set("UserName", currentUser.FirstName);
        Preferences.Set("UserFirstName", currentUser.FirstName);
        Preferences.Set("UserLastName", currentUser.LastName);
        Preferences.Set("UserRole", currentUser.Role);

        _currentEmail = currentUser.Email;

        LoadCurrentUser();

        await ShowAlert(
            "Success",
            "Profile updated successfully.");

        SettingsMainView.IsVisible = true;
        EditProfileView.IsVisible = false;
    }

    private void ShowModal(string title, View content)
    {
        ModalTitleLabel.Text = title;

        ModalContentLayout.Children.Clear();
        ModalContentLayout.Children.Add(content);

        ModalOverlay.IsVisible = true;
    }

    private void OnCloseModalClicked(object? sender, EventArgs e)
    {
        ModalOverlay.IsVisible = false;
    }

    private void OnHelpCenterClicked(object sender, EventArgs e)
    {
        var content = new VerticalStackLayout
        {
            Spacing = 14,
            Children =
            {
                SectionTitle("Getting Started"),
                BodyText("TaskRoster helps employees view schedules, track assigned tasks, and stay updated with reminders."),

                SectionTitle("Viewing Shifts"),
                BodyText("Use the Home screen to check today's shifts and upcoming shifts."),

                SectionTitle("Completing Tasks"),
                BodyText("Assigned tasks are shown on your dashboard. Complete them based on priority and instructions."),

                SectionTitle("Notifications"),
                BodyText("Tap the bell icon to view your employee notifications."),

                BodyText("Need more help? Contact support for assistance.")
            }
        };

        ShowModal("Help Center", content);
    }

    private void OnAboutClicked(object sender, EventArgs e)
    {
        var content = new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                new Label
                {
                    Text = "TaskRoster",
                    FontSize = 22,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#16A34A"),
                    HorizontalTextAlignment = TextAlignment.Center
                },

                new Label
                {
                    Text = "Version 1.0.0",
                    FontSize = 12,
                    TextColor = Color.FromArgb("#6B7280"),
                    HorizontalTextAlignment = TextAlignment.Center
                },

                BodyText("TaskRoster is an employee scheduling and task management application designed to help teams manage shifts, tasks, reminders, and workplace updates."),

                new Label
                {
                    Text = "Employee Features:",
                    FontSize = 14,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#111827")
                },

                BodyText("• View today's shifts\n• View upcoming shifts\n• Track assigned tasks\n• View employee notifications\n• Contact support"),

                BodyText("© 2025 TaskRoster. All rights reserved.")
            }
        };

        ShowModal("About TaskRoster", content);
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool confirm =
            await ShowConfirm(
                "Log Out",
                "Are you sure you want to log out?");

        if (!confirm)
            return;

        Preferences.Set("IsLoggedIn", false);
        Preferences.Remove("UserRole");
        Preferences.Remove("UserName");
        Preferences.Remove("UserFirstName");
        Preferences.Remove("UserLastName");
        Preferences.Remove("UserEmail");

        Application.Current!.Windows[0].Page =
            new NavigationPage(new SignInPage());
    }

    private string GetInitial(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "";

        return value.Trim()[0].ToString().ToUpper();
    }

    private Label SectionTitle(string text)
    {
        return new Label
        {
            Text = text,
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#16A34A")
        };
    }

    private Label BodyText(string text)
    {
        return new Label
        {
            Text = text,
            FontSize = 13,
            TextColor = Color.FromArgb("#4B5563"),
            LineHeight = 1.35
        };
    }

    private async Task ShowAlert(string title, string message)
    {
        Page? page = Window?.Page;

        if (page != null)
            await page.DisplayAlert(title, message, "OK");
    }

    private async Task<bool> ShowConfirm(string title, string message)
    {
        Page? page = Window?.Page;

        if (page != null)
        {
            return await page.DisplayAlert(
                title,
                message,
                "Yes",
                "No");
        }

        return false;
    }
}