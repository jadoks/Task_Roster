using Microsoft.Maui.Controls.Shapes;
using Task_Roster.Views;

namespace Task_Roster.Views.DashboardTabs;

public partial class SettingsView : ContentView
{
    public SettingsView()
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

    private void OnImportDataClicked(object sender, EventArgs e)
    {
        var picker = new Picker
        {
            ItemsSource = new string[] { "Employees", "Schedules", "Tasks" },
            SelectedIndex = 0
        };

        var content = new VerticalStackLayout
        {
            Spacing = 16,
            Children =
            {
                new Label
                {
                    Text = "Select Data Type",
                    FontSize = 14,
                    TextColor = Color.FromArgb("#374151")
                },

                picker,

                new Border
                {
                    BackgroundColor = Color.FromArgb("#DCFCE7"),
                    Stroke = Color.FromArgb("#16A34A"),
                    Padding = 12,
                    StrokeShape = new RoundRectangle { CornerRadius = 6 },
                    Content = new Label
                    {
                        Text = "CSV Format Required:\nColumns: Name, Email, Skills, MaxHours",
                        FontSize = 12,
                        TextColor = Color.FromArgb("#166534")
                    }
                },

                new Button
                {
                    Text = "Choose File",
                    BackgroundColor = Color.FromArgb("#16A34A"),
                    TextColor = Colors.White,
                    CornerRadius = 6,
                    HeightRequest = 52
                }
            }
        };

        ShowModal("Import Data", content);
    }

    private void OnExportDataClicked(object sender, EventArgs e)
    {
        var content = new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                CreateExportCard("Export as CSV", "Multiple files for spreadsheet software", "#DCFCE7"),
                CreateExportCard("Export as PDF", "Single formatted document", "#FEE2E2")
            }
        };

        ShowModal("Choose Export Format", content);
    }

    private View CreateExportCard(string title, string desc, string bgColor)
    {
        return new Border
        {
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#E5E7EB"),
            Padding = 16,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Content = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition()
                },
                ColumnSpacing = 14,
                Children =
                {
                    new Border
                    {
                        WidthRequest = 38,
                        HeightRequest = 38,
                        BackgroundColor = Color.FromArgb(bgColor),
                        StrokeThickness = 0,
                        StrokeShape = new RoundRectangle { CornerRadius = 19 },
                        Content = new Label
                        {
                          
                            HorizontalOptions = LayoutOptions.Center,
                            VerticalOptions = LayoutOptions.Center
                        }
                    },

                    CreateExportTextStack(title, desc)
                }
            }
        };
    }

    private View CreateExportTextStack(string title, string desc)
    {
        var stack = new VerticalStackLayout
        {
            Spacing = 3,
            Children =
            {
                new Label
                {
                    Text = title,
                    FontSize = 14,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#111827")
                },
                new Label
                {
                    Text = desc,
                    FontSize = 12,
                    TextColor = Color.FromArgb("#6B7280")
                }
            }
        };

        Grid.SetColumn(stack, 1);
        return stack;
    }

    private async void OnReportsClicked(object sender, EventArgs e)
    {
        await ShowAlert("Reports & Analytics", "Reports page clicked.");
    }

    private void OnHelpCenterClicked(object sender, EventArgs e)
    {
        var content = new VerticalStackLayout
        {
            Spacing = 14,
            Children =
            {
                SectionTitle("Getting Started"),
                BodyText("TaskRoster makes it easy to manage your team's schedules and tasks. Start by checking your upcoming shifts and assigned tasks on the dashboard."),

                SectionTitle("Managing Shifts"),
                BodyText("You can view all your shifts in the Shifts tab. Accept or decline shifts by tapping on them and using the action buttons."),

                SectionTitle("Completing Tasks"),
                BodyText("Tasks can be marked as complete from the task details page. High priority tasks are highlighted in red for your attention."),

                SectionTitle("Notifications"),
                BodyText("You'll receive notifications about new shifts, tasks, and updates. Customize your notification preferences in the settings."),

                BodyText("Need more help? Contact our support team for assistance.")
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

                BodyText("TaskRoster is a comprehensive employee scheduling and task management application designed to streamline workforce operations for businesses of all sizes."),

                new Label
                {
                    Text = "Key Features:",
                    FontSize = 14,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#111827")
                },

                BodyText("• Intuitive shift scheduling and management\n• Task assignment and tracking\n• Employee availability management\n• Real-time notifications and updates\n• Comprehensive reporting and analytics"),

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
        {
            await page.DisplayAlert(title, message, "OK");
        }
    }

    private async Task<bool> ShowConfirm(string title, string message)
    {
        Page? page = Window?.Page;

        if (page != null)
        {
            return await page.DisplayAlert(title, message, "Yes", "No");
        }

        return false;
    }
}