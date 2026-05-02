using Microsoft.Maui.Controls.Shapes;
using Task_Roster.Models;
using Task_Roster.Services;
using Task_Roster.Views;

namespace Task_Roster.Views.DashboardTabs;

public partial class HomeView : ContentView
{
    private readonly DatabaseService _databaseService;

    public HomeView()
    {
        InitializeComponent();

        _databaseService = new DatabaseService();

        LoadDashboard();
    }

    private async void LoadDashboard()
    {
        TodayDateLabel.Text = DateTime.Today.ToString("ddd, MMM dd");

        await LoadStatsAsync();
        await LoadTodayScheduleAsync();
        await LoadManagerNotificationBadgeAsync();
    }

    private async void OnCreateScheduleTapped(object sender, TappedEventArgs e)
    {
        var scheduleView = new ScheduleView();

        var page = new ContentPage
        {
            BackgroundColor = Color.FromArgb("#F5F6F8"),
            Content = scheduleView
        };

        Page? currentPage = Window?.Page ?? Application.Current?.Windows[0].Page;

        if (currentPage != null)
        {
            await currentPage.Navigation.PushModalAsync(page);
            scheduleView.OpenAddShiftFromQuickAction();
        }
    }

    private async void OnManageTeamTapped(object sender, TappedEventArgs e)
    {
        var teamView = new TeamView();

        var page = new ContentPage
        {
            BackgroundColor = Color.FromArgb("#F5F6F8"),
            Content = teamView
        };

        Page? currentPage = Window?.Page ?? Application.Current?.Windows[0].Page;

        if (currentPage != null)
        {
            await currentPage.Navigation.PushModalAsync(page);
            teamView.OpenAddEmployeeFromQuickAction();
        }
    }

    private async Task LoadStatsAsync()
    {
        var shifts = await _databaseService.GetShiftsAsync();

        var todayShifts = shifts
            .Where(s => s.Date.Date == DateTime.Today)
            .ToList();

        int pendingShifts = shifts.Count(s => s.Status == "Pending");

        int unassigned = shifts.Count(s =>
            string.IsNullOrWhiteSpace(s.Employee) ||
            s.Employee == "-- Unassigned --");

        int staffToday = todayShifts
            .Where(s =>
                !string.IsNullOrWhiteSpace(s.Employee) &&
                s.Employee != "-- Unassigned --")
            .Select(s => s.Employee)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();

        int totalTasks = 0;
        int completedTasks = 0;

        foreach (var shift in shifts)
        {
            var tasks = await _databaseService.GetTasksByShiftIdAsync(shift.Id);

            totalTasks += tasks.Count;
            completedTasks += tasks.Count(t => t.IsCompleted);
        }

        PendingShiftsLabel.Text = pendingShifts.ToString();
        UnassignedLabel.Text = unassigned.ToString();
        StaffTodayLabel.Text = staffToday.ToString();
        TasksDoneLabel.Text = $"{completedTasks}/{totalTasks}";
    }

    private async Task LoadTodayScheduleAsync()
    {
        TodayScheduleLayout.Children.Clear();

        var shifts = await _databaseService.GetShiftsAsync();

        var todayShifts = shifts
            .Where(s => s.Date.Date == DateTime.Today)
            .OrderBy(s => s.StartTime)
            .ToList();

        if (todayShifts.Count == 0)
        {
            TodayScheduleLayout.Children.Add(CreateEmptyTodayScheduleCard());
            return;
        }

        foreach (var shift in todayShifts)
        {
            TodayScheduleLayout.Children.Add(CreateShiftCard(shift));
        }
    }

    private View CreateEmptyTodayScheduleCard()
    {
        var createButton = new Button
        {
            Text = "Create Shifts",
            BackgroundColor = Color.FromArgb("#2563EB"),
            TextColor = Colors.White,
            FontSize = 12,
            CornerRadius = 6,
            Padding = new Thickness(18, 8)
        };

        createButton.Clicked += (_, _) =>
        {
            OnCreateScheduleTapped(this, new TappedEventArgs(null));
        };

        return new Border
        {
            BackgroundColor = Colors.White,
            Padding = 20,
            Stroke = Color.FromArgb("#E5E7EB"),
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Content = new VerticalStackLayout
            {
                Spacing = 16,
                HorizontalOptions = LayoutOptions.Center,
                Children =
                {
                    new Label
                    {
                        Text = "No shifts scheduled for today",
                        FontSize = 12,
                        TextColor = Color.FromArgb("#6B7280"),
                        HorizontalTextAlignment = TextAlignment.Center
                    },
                    createButton
                }
            }
        };
    }

    private View CreateShiftCard(ShiftModel shift)
    {
        return new Border
        {
            BackgroundColor = Colors.White,
            Padding = 14,
            Stroke = Color.FromArgb("#E5E7EB"),
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Content = new VerticalStackLayout
            {
                Spacing = 8,
                Children =
                {
                    new Grid
                    {
                        ColumnDefinitions =
                        {
                            new ColumnDefinition(),
                            new ColumnDefinition { Width = GridLength.Auto }
                        },
                        Children =
                        {
                            new Label
                            {
                                Text = shift.Role,
                                FontSize = 14,
                                FontAttributes = FontAttributes.Bold,
                                TextColor = Color.FromArgb("#111827")
                            },
                            CreateStatusBadge(shift.Status)
                        }
                    },
                    new Label
                    {
                        Text = $"🕘 {FormatTime(shift.StartTime)} - {FormatTime(shift.EndTime)}",
                        FontSize = 12,
                        TextColor = Color.FromArgb("#6B7280")
                    },
                    new Label
                    {
                        Text = $"📍 {shift.Location}",
                        FontSize = 12,
                        TextColor = Color.FromArgb("#6B7280")
                    },
                    new Label
                    {
                        Text = $"👤 {(string.IsNullOrWhiteSpace(shift.Employee) || shift.Employee == "-- Unassigned --" ? "Unassigned" : shift.Employee)}",
                        FontSize = 12,
                        TextColor = Color.FromArgb("#6B7280")
                    }
                }
            }
        };
    }

    private View CreateStatusBadge(string status)
    {
        var badge = new Border
        {
            Padding = new Thickness(10, 4),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            BackgroundColor = status switch
            {
                "Accepted" => Color.FromArgb("#DCFCE7"),
                "Declined" => Color.FromArgb("#FEE2E2"),
                _ => Color.FromArgb("#FEF3C7")
            },
            Content = new Label
            {
                Text = status,
                FontSize = 10,
                TextColor = status switch
                {
                    "Accepted" => Color.FromArgb("#166534"),
                    "Declined" => Color.FromArgb("#DC2626"),
                    _ => Color.FromArgb("#CA8A04")
                }
            }
        };

        Grid.SetColumn(badge, 1);
        return badge;
    }

    private async Task LoadManagerNotificationBadgeAsync()
    {
        int count = await GetManagerNotificationCountAsync();

        ManagerNotificationBadge.IsVisible = count > 0;
        ManagerNotificationBadgeLabel.Text = count > 99 ? "99+" : count.ToString();
    }

    private async Task<int> GetManagerNotificationCountAsync()
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
            count += tasks.Count(t => t.IsCompleted);
        }

        return count;
    }

    private async void OnBellTapped(object sender, TappedEventArgs e)
    {
        Page? page = Window?.Page ?? Application.Current?.Windows[0].Page;

        if (page != null)
        {
            await page.Navigation.PushModalAsync(new ManagerNotification());
            await LoadManagerNotificationBadgeAsync();
        }
    }

    private static string FormatTime(TimeSpan time)
    {
        return DateTime.Today
            .Add(time)
            .ToString("h:mm tt");
    }
}
