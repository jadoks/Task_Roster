using Microsoft.Maui.Controls.Shapes;
using Task_Roster.Views;

namespace Task_Roster.Views.EmployeeDashboardTabs;

public partial class EmployeeSettingsView : ContentView
{
    public EmployeeSettingsView()
    {
        InitializeComponent();
    }

    private void OnEditProfileClicked(object sender, EventArgs e)
    {
        SettingsMainView.IsVisible = false;
        EditProfileView.IsVisible = true;
        NotificationView.IsVisible = false;
    }

    private void OnNotificationSettingsClicked(object sender, EventArgs e)
    {
        SettingsMainView.IsVisible = false;
        EditProfileView.IsVisible = false;
        NotificationView.IsVisible = true;
    }

    private void OnBackToSettingsClicked(object sender, EventArgs e)
    {
        SettingsMainView.IsVisible = true;
        EditProfileView.IsVisible = false;
        NotificationView.IsVisible = false;
    }

    private async void OnProfileCameraTapped(object sender, TappedEventArgs e)
    {
        try
        {
            FileResult? photo = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select Profile Picture",
                FileTypes = FilePickerFileType.Images
            });

            if (photo == null)
                return;

            using Stream sourceStream = await photo.OpenReadAsync();
            using MemoryStream memoryStream = new();

            await sourceStream.CopyToAsync(memoryStream);

            byte[] imageBytes = memoryStream.ToArray();

            ProfileImage.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));
            EditProfileImage.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));

            ProfileImage.IsVisible = true;
            EditProfileImage.IsVisible = true;

            ProfileInitialsLabel.IsVisible = false;
            EditProfileInitialsLabel.IsVisible = false;
        }
        catch
        {
            await ShowAlert("Error", "Unable to select profile picture.");
        }
    }

    private async void OnSaveProfileClicked(object sender, EventArgs e)
    {
        ProfileNameLabel.Text = EditNameEntry.Text;
        ProfileEmailLabel.Text = EditEmailEntry.Text;

        await ShowAlert("Success", "Profile updated successfully.");
    }

    private async void OnSaveNotificationsClicked(object sender, EventArgs e)
    {
        await ShowAlert("Saved", "Notification preferences updated.");
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
                BodyText("You can manage shift reminders, task reminders, schedule updates, email notifications, and push notifications in Settings."),

                BodyText("Need more help? Contact support for assistance.")
            }
        };

        ShowModal("Help Center", content);
    }

    private void OnContactSupportClicked(object sender, EventArgs e)
    {
        var buttonGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(),
                new ColumnDefinition()
            },
            ColumnSpacing = 12
        };

        var cancelButton = new Button
        {
            Text = "Cancel",
            BackgroundColor = Colors.White,
            TextColor = Color.FromArgb("#374151"),
            BorderColor = Color.FromArgb("#D1D5DB"),
            BorderWidth = 1,
            CornerRadius = 6
        };

        cancelButton.Clicked += OnCloseModalClicked;

        var submitButton = new Button
        {
            Text = "Submit",
            BackgroundColor = Color.FromArgb("#14532D"),
            TextColor = Colors.White,
            CornerRadius = 6
        };

        Grid.SetColumn(submitButton, 1);

        buttonGrid.Children.Add(cancelButton);
        buttonGrid.Children.Add(submitButton);

        var content = new VerticalStackLayout
        {
            Spacing = 14,
            Children =
            {
                BodyText("📍 143 TaskRoster St., Cebu City, Philippines"),
                BodyText("📞 +63 912 345 6789"),
                BodyText("✉ support@taskroster.com"),
                BodyText("For urgent inquiries, please call our hotline."),

                new BoxView
                {
                    HeightRequest = 1,
                    Color = Color.FromArgb("#D1D5DB")
                },

                new Entry
                {
                    Placeholder = "Name",
                    BackgroundColor = Colors.White
                },

                new Entry
                {
                    Placeholder = "Email",
                    Keyboard = Keyboard.Email,
                    BackgroundColor = Colors.White
                },

                new Editor
                {
                    Placeholder = "Message",
                    HeightRequest = 100,
                    BackgroundColor = Colors.White
                },

                buttonGrid
            }
        };

        ShowModal("Contact Support", content);
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

                BodyText("• View today's shifts\n• View upcoming shifts\n• Track assigned tasks\n• Manage notification preferences\n• Contact support"),

                BodyText("© 2025 TaskRoster. All rights reserved.")
            }
        };

        ShowModal("About TaskRoster", content);
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool confirm = await ShowConfirm("Log Out", "Are you sure you want to log out?");

        if (!confirm)
            return;

        Preferences.Set("IsLoggedIn", false);
        Preferences.Remove("UserRole");

        Application.Current!.Windows[0].Page = new NavigationPage(new SignInPage());
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
            return await page.DisplayAlert(title, message, "Yes", "No");

        return false;
    }
}