using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Dispatching;
using System.Text;
using Task_Roster.Models;
using Task_Roster.Services;
using Task_Roster.Views;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Graphics;
using Microcharts;
using Microcharts.Maui;
using SkiaSharp;
using MauiColor = Microsoft.Maui.Graphics.Color;

namespace Task_Roster.Views.DashboardTabs;

public partial class SettingsView : ContentView
{
    private readonly DatabaseService _databaseService;
    private IDispatcherTimer? _badgeTimer;

    private string _currentEmail = "";
    private byte[]? _selectedProfileImageBytes;

    public SettingsView()
    {
        InitializeComponent();

        _databaseService = new DatabaseService();

        LoadCurrentUser();
        StartNotificationAutoRefresh();
    }

    private async void LoadCurrentUser()
    {
        string email = Preferences.Get("UserEmail", "");

        if (string.IsNullOrWhiteSpace(email))
            return;

        UserModel? user = await _databaseService.GetUserByEmailAsync(email);

        if (user == null)
            return;

        _currentEmail = user.Email;
        _selectedProfileImageBytes = user.ProfileImageBytes;

        string fullName = $"{user.FirstName} {user.LastName}".Trim();
        string initials = $"{GetInitial(user.FirstName)}{GetInitial(user.LastName)}";

        ProfileNameLabel.Text = fullName;
        ProfileEmailLabel.Text = user.Email;

        EditNameEntry.Text = fullName;
        EditEmailEntry.Text = user.Email;

        ProfileInitialsLabel.Text = initials;
        EditProfileInitialsLabel.Text = initials;

        SetProfileImage(user.ProfileImageBytes);

        await RefreshNotificationBadge();
    }

    private void StartNotificationAutoRefresh()
    {
        _badgeTimer?.Stop();

        _badgeTimer = Dispatcher.CreateTimer();
        _badgeTimer.Interval = TimeSpan.FromSeconds(5);

        _badgeTimer.Tick += async (_, _) =>
        {
            await RefreshNotificationBadge();
        };

        _badgeTimer.Start();
    }

    private async Task RefreshNotificationBadge()
    {
        int count = await GetNotificationCount();

        bool visible = count > 0;
        string text = count > 99 ? "99+" : count.ToString();

        MainNotificationBadge.IsVisible = visible;
        EditNotificationBadge.IsVisible = visible;
        PrefsNotificationBadge.IsVisible = visible;

        MainNotificationBadgeLabel.Text = text;
        EditNotificationBadgeLabel.Text = text;
        PrefsNotificationBadgeLabel.Text = text;
    }

    private async Task<int> GetNotificationCount()
    {
        int count = 0;

        var shifts = await _databaseService.GetShiftsAsync();

        foreach (var shift in shifts)
        {
            if (shift.Status == "Pending")
                count++;

            if (string.IsNullOrWhiteSpace(shift.Employee) ||
                shift.Employee == "-- Unassigned --")
                count++;

            var tasks = await _databaseService.GetTasksByShiftIdAsync(shift.Id);
            count += tasks.Count(t => !t.IsCompleted);
        }

        return count;
    }

    private async void OnBellTapped(object sender, TappedEventArgs e)
    {
        Page? page = Window?.Page ?? Application.Current?.Windows[0].Page;

        if (page != null)
        {
            await page.Navigation.PushModalAsync(new ManagerNotification());
            await RefreshNotificationBadge();
        }
    }

    private void SetProfileImage(byte[]? imageBytes)
    {
        if (imageBytes != null && imageBytes.Length > 0)
        {
            ProfileImage.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));
            EditProfileImage.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));

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
        NotificationView.IsVisible = false;
    }

    private void OnBackToSettingsClicked(object sender, EventArgs e)
    {
        SettingsMainView.IsVisible = true;
        EditProfileView.IsVisible = false;
        NotificationView.IsVisible = false;

        LoadCurrentUser();
    }

    private async void OnProfileCameraTapped(object sender, TappedEventArgs e)
    {
        try
        {
            FileResult? photo = await FilePicker.Default.PickAsync(
                new PickOptions
                {
                    PickerTitle = "Select Profile Picture",
                    FileTypes = FilePickerFileType.Images
                });

            if (photo == null)
                return;

            using Stream sourceStream = await photo.OpenReadAsync();
            using MemoryStream memoryStream = new();

            await sourceStream.CopyToAsync(memoryStream);

            _selectedProfileImageBytes = memoryStream.ToArray();

            SetProfileImage(_selectedProfileImageBytes);
        }
        catch
        {
            await ShowAlert("Error", "Unable to select profile picture.");
        }
    }

    private async void OnSaveProfileClicked(object sender, EventArgs e)
    {
        string fullName = EditNameEntry.Text?.Trim() ?? "";
        string newEmail = EditEmailEntry.Text?.Trim().ToLower() ?? "";

        if (string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(newEmail))
        {
            await ShowAlert("Missing Information", "Please complete all fields.");
            return;
        }

        UserModel? currentUser = await _databaseService.GetUserByEmailAsync(_currentEmail);

        if (currentUser == null)
        {
            await ShowAlert("Error", "Current user was not found.");
            return;
        }

        string[] nameParts = fullName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

        currentUser.FirstName = nameParts.Length > 0 ? nameParts[0] : "";
        currentUser.LastName = nameParts.Length > 1 ? nameParts[1] : "";
        currentUser.Email = newEmail;
        currentUser.ProfileImageBytes = _selectedProfileImageBytes;

        await _databaseService.UpdateUserAsync(currentUser);

        Preferences.Set("UserEmail", currentUser.Email);
        Preferences.Set("UserName", currentUser.FirstName);
        Preferences.Set("UserFirstName", currentUser.FirstName);
        Preferences.Set("UserLastName", currentUser.LastName);
        Preferences.Set("UserRole", currentUser.Role);

        _currentEmail = currentUser.Email;

        LoadCurrentUser();

        await ShowAlert("Success", "Profile updated successfully.");

        SettingsMainView.IsVisible = true;
        EditProfileView.IsVisible = false;
        NotificationView.IsVisible = false;
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

    // =========================
    // IMPORT DATA
    // =========================

    private async void OnImportDataClicked(object sender, EventArgs e)
    {
        try
        {
            FileResult? file = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select CSV File"
            });

            if (file == null)
                return;

            using Stream stream = await file.OpenReadAsync();
            using StreamReader reader = new(stream);

            string csv = await reader.ReadToEndAsync();

            string[] rows = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            if (rows.Length <= 1)
            {
                await ShowAlert("Import Error", "CSV file is empty.");
                return;
            }

            int importedCount = 0;

            // Skip header row
            for (int i = 1; i < rows.Length; i++)
            {
                string[] columns = rows[i].Split(',');

                if (columns.Length < 3)
                    continue;

                ShiftModel shift = new()
                {
                    Employee = columns[0].Trim(),
                    Status = columns[1].Trim(),
                    Date = DateTime.TryParse(columns[2], out DateTime date)
                        ? date
                        : DateTime.Now
                };

                await _databaseService.AddShiftAsync(shift);

                importedCount++;
            }

            await ShowAlert(
                "Import Successful",
                $"Successfully imported {importedCount} records into SQLite database.");
        }
        catch (Exception ex)
        {
            await ShowAlert("Import Error", ex.Message);
        }
    }

    // =========================
    // EXPORT DATA
    // =========================

    private void OnExportDataClicked(object sender, EventArgs e)
    {
        var layout = new VerticalStackLayout
        {
            Spacing = 14
        };

        layout.Children.Add(CreateExportButton("Export as CSV", "#2563EB", async () =>
        {
            await ExportCsv();
        }));

        layout.Children.Add(CreateExportButton("Export as XLS", "#16A34A", async () =>
        {
            await ExportXls();
        }));

        layout.Children.Add(CreateExportButton("Export as PDF", "#DC2626", async () =>
        {
            await ExportPdf();
        }));

        ShowModal("Choose Export Format", layout);
    }

    private View CreateExportButton(
    string text,
    string color,
    Func<Task> action)
    {
        Button button = new()
        {
            Text = text,
            BackgroundColor = MauiColor.FromArgb(color),
            TextColor = MauiColor.FromArgb("#FFFFFF"),
            CornerRadius = 8,
            HeightRequest = 50
        };

        button.Clicked += async (_, _) =>
        {
            ModalOverlay.IsVisible = false;
            await action();
        };

        return button;
    }

    private async Task ExportCsv()
    {
        try
        {
            var shifts = await _databaseService.GetShiftsAsync();

            StringBuilder builder = new();

            builder.AppendLine("Employee,Status,Date");

            foreach (var shift in shifts)
            {
                builder.AppendLine(
                    $"{shift.Employee},{shift.Status},{shift.Date:yyyy-MM-dd}");
            }

            string fileName =
                $"TaskRoster_{DateTime.Now:yyyyMMddHHmmss}.csv";

            string filePath =
                System.IO.Path.Combine(FileSystem.CacheDirectory, fileName);

            File.WriteAllText(filePath, builder.ToString());

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Export CSV",
                File = new ShareFile(filePath)
            });

            await ShowAlert("Success", "CSV exported successfully.");
        }
        catch (Exception ex)
        {
            await ShowAlert("Export Error", ex.Message);
        }
    }

    private async Task ExportXls()
    {
        try
        {
            var shifts = await _databaseService.GetShiftsAsync();

            StringBuilder builder = new();

            builder.AppendLine("Employee\tStatus\tDate");

            foreach (var shift in shifts)
            {
                builder.AppendLine(
                    $"{shift.Employee}\t{shift.Status}\t{shift.Date:yyyy-MM-dd}");
            }

            string fileName =
                $"TaskRoster_{DateTime.Now:yyyyMMddHHmmss}.xls";

            string filePath =
                System.IO.Path.Combine(FileSystem.CacheDirectory, fileName);

            File.WriteAllText(filePath, builder.ToString());

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Export XLS",
                File = new ShareFile(filePath)
            });

            await ShowAlert("Success", "XLS exported successfully.");
        }
        catch (Exception ex)
        {
            await ShowAlert("Export Error", ex.Message);
        }
    }

    private View CreateExportOption(
    string title,
    string description,
    string color,
    Func<Task> action)
    {
        Border card = new()
        {
            BackgroundColor = MauiColor.FromArgb("#FFFFFF"),
            Stroke = MauiColor.FromArgb("#E5E7EB"),
            Padding = 16,

            StrokeShape = new RoundRectangle
            {
                CornerRadius = 8
            }
        };

        Grid grid = new()
        {
            ColumnDefinitions =
        {
            new ColumnDefinition { Width = GridLength.Auto },
            new ColumnDefinition { Width = GridLength.Star }
        },

            ColumnSpacing = 14
        };

        Border icon = new()
        {
            WidthRequest = 42,
            HeightRequest = 42,
            BackgroundColor = MauiColor.FromArgb(color),
            StrokeThickness = 0,

            StrokeShape = new RoundRectangle
            {
                CornerRadius = 21
            }
        };

        VerticalStackLayout textStack = new()
        {
            Spacing = 3
        };

        textStack.Children.Add(new Label
        {
            Text = title,
            FontSize = 15,
            FontAttributes = FontAttributes.Bold,
            TextColor = MauiColor.FromArgb("#111827")
        });

        textStack.Children.Add(new Label
        {
            Text = description,
            FontSize = 12,
            TextColor = MauiColor.FromArgb("#6B7280")
        });

        Grid.SetColumn(textStack, 1);

        grid.Children.Add(icon);
        grid.Children.Add(textStack);

        card.Content = grid;

        TapGestureRecognizer tap = new();

        tap.Tapped += async (_, _) =>
        {
            ModalOverlay.IsVisible = false;
            await action();
        };

        card.GestureRecognizers.Add(tap);

        return card;
    }

    private async Task ExportDataAsync(string format)
    {
        try
        {
            var shifts = await _databaseService.GetShiftsAsync();
            var tasks = await _databaseService.GetTasksAsync();

            StringBuilder builder = new();

            builder.AppendLine("TASKROSTER REPORT");
            builder.AppendLine();

            builder.AppendLine("=== SHIFTS ===");
            builder.AppendLine("Employee,Status,Date");

            foreach (var shift in shifts)
            {
                builder.AppendLine(
                    $"{shift.Employee},{shift.Status},{shift.Date:yyyy-MM-dd}");
            }

            builder.AppendLine();

            builder.AppendLine("=== TASKS ===");
            builder.AppendLine("Title,Completed");

            foreach (var task in tasks)
            {
                builder.AppendLine(
                    $"{task.Title},{task.IsCompleted}");
            }

            string extension = format.ToLower();

            string fileName =
                $"TaskRoster_Report_{DateTime.Now:yyyyMMddHHmmss}.{extension}";

            string filePath =
                System.IO.Path.Combine(FileSystem.CacheDirectory, fileName);

            File.WriteAllText(filePath, builder.ToString());

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Export TaskRoster Data",
                File = new ShareFile(filePath)
            });

            await ShowAlert(
                "Export Successful",
                $"Data exported successfully as {extension.ToUpper()}.");
        }
        catch (Exception ex)
        {
            await ShowAlert("Export Error", ex.Message);
        }
    }

    private async Task ExportPdf()
    {
        try
        {
            var shifts = await _databaseService.GetShiftsAsync();

            StringBuilder builder = new();

            builder.AppendLine("TASKROSTER REPORT");
            builder.AppendLine("======================");
            builder.AppendLine();

            foreach (var shift in shifts)
            {
                builder.AppendLine(
                    $"{shift.Employee} | {shift.Status} | {shift.Date:yyyy-MM-dd}");
            }

            string fileName =
                $"TaskRoster_{DateTime.Now:yyyyMMddHHmmss}.pdf";

            string filePath =
                System.IO.Path.Combine(FileSystem.CacheDirectory, fileName);

            File.WriteAllText(filePath, builder.ToString());

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Export PDF",
                File = new ShareFile(filePath)
            });

            await ShowAlert("Success", "PDF exported successfully.");
        }
        catch (Exception ex)
        {
            await ShowAlert("PDF Export Error", ex.Message);
        }
    }

    // =========================
    // ANALYTICS / REPORTS
    // =========================

    private async void OnReportsClicked(object sender, EventArgs e)
    {
        try
        {
            var shifts = await _databaseService.GetShiftsAsync();

            int pending = shifts.Count(s => s.Status == "Pending");
            int approved = shifts.Count(s => s.Status == "Approved");
            int rejected = shifts.Count(s => s.Status == "Rejected");

            int totalTasks = 0;
            int completedTasks = 0;

            foreach (var shift in shifts)
            {
                var tasks = await _databaseService.GetTasksByShiftIdAsync(shift.Id);

                totalTasks += tasks.Count;
                completedTasks += tasks.Count(t => t.IsCompleted);
            }

            int incompleteTasks = totalTasks - completedTasks;

            var shiftChart = new ChartView
            {
                HeightRequest = 220,
                Chart = new DonutChart
                {
                    Entries = new[]
                    {
                    new ChartEntry(pending)
                    {
                        Label = "Pending",
                        ValueLabel = pending.ToString(),
                        Color = SKColor.Parse("#F59E0B")
                    },

                    new ChartEntry(approved)
                    {
                        Label = "Approved",
                        ValueLabel = approved.ToString(),
                        Color = SKColor.Parse("#10B981")
                    },

                    new ChartEntry(rejected)
                    {
                        Label = "Rejected",
                        ValueLabel = rejected.ToString(),
                        Color = SKColor.Parse("#EF4444")
                    }
                }
                }
            };

            var taskChart = new ChartView
            {
                HeightRequest = 220,
                Chart = new BarChart
                {
                    Entries = new[]
                    {
                    new ChartEntry(completedTasks)
                    {
                        Label = "Completed",
                        ValueLabel = completedTasks.ToString(),
                        Color = SKColor.Parse("#2563EB")
                    },

                    new ChartEntry(incompleteTasks)
                    {
                        Label = "Incomplete",
                        ValueLabel = incompleteTasks.ToString(),
                        Color = SKColor.Parse("#DC2626")
                    }
                }
                }
            };

            var content = new VerticalStackLayout
            {
                Spacing = 24,
                Children =
            {
                new Label
                {
                    Text = "Shift Status Overview",
                    FontSize = 18,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = MauiColor.FromArgb("#111827")
                },

                shiftChart,

                new Label
                {
                    Text = "Task Completion Analytics",
                    FontSize = 18,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = MauiColor.FromArgb("#111827")
                },

                taskChart
            }
            };

            ShowModal("Reports & Analytics", content);
        }
        catch (Exception ex)
        {
            await ShowAlert("Analytics Error", ex.Message);
        }
    }

    private View CreateAnalyticsCard(string title, string value, string bgColor)
    {
        return new Border
        {
            BackgroundColor = MauiColor.FromArgb(bgColor),
            StrokeThickness = 0,
            Padding = 16,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },

            Content = new VerticalStackLayout
            {
                Spacing = 4,
                Children =
                {
                    new Label
                    {
                        Text = title,
                        FontSize = 13,
                        TextColor = MauiColor.FromArgb("#6B7280")
                    },

                    new Label
                    {
                        Text = value,
                        FontSize = 26,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = MauiColor.FromArgb("#111827")
                    }
                }
            }
        };
    }

    private View CreateChartBar(
    string title,
    object value,
    double width,
    string color)
    {
        return new VerticalStackLayout
        {
            Spacing = 6,

            Children =
        {
            new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto)
                },

                Children =
                {
                    new Label
                    {
                        Text = title,
                        FontSize = 13,
                        TextColor = MauiColor.FromArgb("#374151")
                    },

                    new Label
                    {
                        Text = value.ToString(),
                        FontSize = 13,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = MauiColor.FromArgb("#111827")
                    }
                }
            },

            new Border
            {
                HeightRequest = 14,
                BackgroundColor = MauiColor.FromArgb("#E5E7EB"),
                StrokeThickness = 0,

                StrokeShape = new RoundRectangle
                {
                    CornerRadius = 7
                },

                Content = new Border
                {
                    WidthRequest = width,
                    HorizontalOptions = LayoutOptions.Start,
                    BackgroundColor = MauiColor.FromArgb(color),
                    StrokeThickness = 0,

                    StrokeShape = new RoundRectangle
                    {
                        CornerRadius = 7
                    }
                }
            }
        }
        };
    }

    private void OnHelpCenterClicked(object sender, EventArgs e)
    {
        var content = new VerticalStackLayout
        {
            Spacing = 14,
            Children =
            {
                SectionTitle("Getting Started"),
                BodyText("TaskRoster makes it easy to manage schedules and tasks."),

                SectionTitle("Managing Shifts"),
                BodyText("View, assign, and manage employee shifts."),

                SectionTitle("Task Management"),
                BodyText("Track tasks and monitor completion progress."),

                SectionTitle("Notifications"),
                BodyText("Receive updates for tasks and schedules.")
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
                    TextColor = MauiColor.FromArgb("#16A34A"),
                    HorizontalTextAlignment = TextAlignment.Center
                },

                new Label
                {
                    Text = "Version 1.0.0",
                    FontSize = 12,
                    TextColor = MauiColor.FromArgb("#6B7280"),
                    HorizontalTextAlignment = TextAlignment.Center
                },

                BodyText("TaskRoster is an employee scheduling and task management application.")
            }
        };

        ShowModal("About TaskRoster", content);
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool confirm = await ShowConfirm("Log Out", "Are you sure you want to log out?");

        if (!confirm)
            return;

        _badgeTimer?.Stop();

        Preferences.Set("IsLoggedIn", false);
        Preferences.Remove("UserRole");
        Preferences.Remove("UserName");
        Preferences.Remove("UserFirstName");
        Preferences.Remove("UserLastName");
        Preferences.Remove("UserEmail");

        Application.Current!.Windows[0].Page = new NavigationPage(new SignInPage());
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
            TextColor = MauiColor.FromArgb("#16A34A")
        };
    }

    private Label BodyText(string text)
    {
        return new Label
        {
            Text = text,
            FontSize = 13,
            TextColor = MauiColor.FromArgb("#4B5563"),
            LineHeight = 1.35
        };
    }

    private async Task ShowAlert(string title, string message)
    {
        Page? page = Window?.Page ?? Application.Current?.Windows[0].Page;

        if (page != null)
        {
            await page.DisplayAlert(title, message, "OK");
        }
    }

    private async Task<bool> ShowConfirm(string title, string message)
    {
        Page? page = Window?.Page ?? Application.Current?.Windows[0].Page;

        if (page != null)
        {
            return await page.DisplayAlert(title, message, "Yes", "No");
        }

        return false;
    }
}