using Microsoft.Maui.Controls.Shapes;
using Task_Roster.Models;
using Task_Roster.Services;

namespace Task_Roster.Views.EmployeeDashboardTabs;

public partial class EmployeeHomeView : ContentView
{
    private readonly DatabaseService _databaseService;

    private UserModel? _currentUser;
    private TaskModel? _selectedTask;
    private ShiftModel? _selectedTaskShift;

    private string _currentFullName = "";
    private string _currentEmail = "";

    public EmployeeHomeView()
    {
        InitializeComponent();

        _databaseService = new DatabaseService();

        LoadEmployeeDashboard();
    }

    private async void LoadEmployeeDashboard()
    {
        DateLabel.Text = DateTime.Now.ToString("ddd, MMM dd");

        await LoadCurrentUserAsync();
        await LoadDashboardDataAsync();
    }

    private async Task LoadCurrentUserAsync()
    {
        string email = Preferences.Get("UserEmail", "");

        if (string.IsNullOrWhiteSpace(email))
        {
            GreetingLabel.Text = "Hi, Employee";
            return;
        }

        _currentUser = await _databaseService.GetUserByEmailAsync(email);

        if (_currentUser == null)
        {
            GreetingLabel.Text = "Hi, Employee";
            return;
        }

        _currentEmail = _currentUser.Email;
        _currentFullName = $"{_currentUser.FirstName} {_currentUser.LastName}".Trim();

        GreetingLabel.Text = string.IsNullOrWhiteSpace(_currentUser.FirstName)
            ? "Hi, Employee"
            : $"Hi, {_currentUser.FirstName}";
    }

    private async Task LoadDashboardDataAsync()
    {
        TodayShiftsLayout.Children.Clear();
        UpcomingShiftsLayout.Children.Clear();
        TasksLayout.Children.Clear();

        var allShifts = await _databaseService.GetShiftsAsync();

        var myShifts = allShifts
            .Where(s => IsAssignedToCurrentEmployee(s.Employee))
            .OrderBy(s => s.Date)
            .ThenBy(s => s.StartTime)
            .ToList();

        DateTime today = DateTime.Today;
        DateTime weekStart = GetStartOfWeek(today);
        DateTime weekEnd = weekStart.AddDays(6);

        var todayShifts = myShifts
            .Where(s => s.Date.Date == today)
            .ToList();

        var weekShifts = myShifts
            .Where(s => s.Date.Date >= weekStart && s.Date.Date <= weekEnd)
            .ToList();

        var upcomingShifts = myShifts
            .Where(s => s.Date.Date > today)
            .Take(5)
            .ToList();

        var allMyTasks = new List<(TaskModel Task, ShiftModel Shift)>();

        foreach (var shift in myShifts)
        {
            var shiftTasks = await _databaseService.GetTasksByShiftIdAsync(shift.Id);

            foreach (var task in shiftTasks)
            {
                allMyTasks.Add((task, shift));
            }
        }

        int completedTasks = allMyTasks.Count(x => x.Task.IsCompleted);
        int totalTasks = allMyTasks.Count;
        int pendingTasks = allMyTasks.Count(x => !x.Task.IsCompleted);

        WeekShiftCountLabel.Text = $"{weekShifts.Count} {(weekShifts.Count == 1 ? "shift" : "shifts")}";
        TaskSummaryLabel.Text = $"{completedTasks}/{totalTasks}";
        PendingTaskBadgeLabel.Text = pendingTasks.ToString();

        RenderTodayShifts(todayShifts);
        RenderUpcomingShifts(upcomingShifts);
        RenderPendingTasks(allMyTasks.Where(x => !x.Task.IsCompleted).ToList());
    }

    private bool IsAssignedToCurrentEmployee(string assignedEmployee)
    {
        if (string.IsNullOrWhiteSpace(assignedEmployee))
            return false;

        if (assignedEmployee == "-- Unassigned --")
            return false;

        return assignedEmployee.Equals(_currentFullName, StringComparison.OrdinalIgnoreCase) ||
               assignedEmployee.Equals(_currentEmail, StringComparison.OrdinalIgnoreCase) ||
               assignedEmployee.Equals(_currentUser?.FirstName, StringComparison.OrdinalIgnoreCase);
    }

    private void RenderTodayShifts(List<ShiftModel> shifts)
    {
        if (shifts.Count == 0)
        {
            TodayShiftsLayout.Children.Add(CreateEmptyCard(
                "No shifts scheduled for today",
                "Enjoy your day off!",
                18));

            return;
        }

        foreach (var shift in shifts)
        {
            TodayShiftsLayout.Children.Add(CreateShiftCard(shift));
        }
    }

    private void RenderUpcomingShifts(List<ShiftModel> shifts)
    {
        if (shifts.Count == 0)
        {
            UpcomingShiftsLayout.Children.Add(CreateEmptyCard(
                "No upcoming shifts scheduled",
                "",
                28));

            return;
        }

        foreach (var shift in shifts)
        {
            UpcomingShiftsLayout.Children.Add(CreateShiftCard(shift));
        }
    }

    private void RenderPendingTasks(List<(TaskModel Task, ShiftModel Shift)> tasks)
    {
        if (tasks.Count == 0)
        {
            TasksLayout.Children.Add(CreateEmptyCard(
                "No tasks to complete",
                "You're all caught up!",
                18));

            return;
        }

        foreach (var item in tasks.Take(5))
        {
            TasksLayout.Children.Add(CreateTaskCard(item.Task, item.Shift));
        }
    }

    private View CreateShiftCard(ShiftModel shift)
    {
        return new Border
        {
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#E5E7EB"),
            Padding = 16,
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
                                FontSize = 15,
                                FontAttributes = FontAttributes.Bold,
                                TextColor = Color.FromArgb("#111827")
                            },
                            CreateStatusBadge(shift.Status)
                        }
                    },

                    new Label
                    {
                        Text = $"📅 {shift.Date:ddd, MMM dd}",
                        FontSize = 13,
                        TextColor = Color.FromArgb("#6B7280")
                    },

                    new Label
                    {
                        Text = $"🕘 {FormatTime(shift.StartTime)} - {FormatTime(shift.EndTime)}",
                        FontSize = 13,
                        TextColor = Color.FromArgb("#6B7280")
                    },

                    new Label
                    {
                        Text = $"📍 {shift.Location}",
                        FontSize = 13,
                        TextColor = Color.FromArgb("#6B7280")
                    }
                }
            }
        };
    }

    private View CreateTaskCard(TaskModel task, ShiftModel shift)
    {
        var card = new Border
        {
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#E5E7EB"),
            Padding = 16,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Content = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(),
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                Children =
                {
                    new VerticalStackLayout
                    {
                        Spacing = 8,
                        Children =
                        {
                            new Label
                            {
                                Text = task.Title,
                                FontSize = 15,
                                TextColor = Color.FromArgb("#111827")
                            },
                            new Label
                            {
                                Text = string.IsNullOrWhiteSpace(task.Description)
                                    ? $"{shift.Role} • {shift.Date:MMM dd}, {FormatTime(shift.StartTime)}"
                                    : task.Description,
                                FontSize = 13,
                                TextColor = Color.FromArgb("#6B7280")
                            }
                        }
                    },

                    CreatePriorityBadge(task.Priority)
                }
            }
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => ShowTaskDetails(task, shift);
        card.GestureRecognizers.Add(tap);

        return card;
    }

    private void ShowTaskDetails(TaskModel task, ShiftModel shift)
    {
        _selectedTask = task;
        _selectedTaskShift = shift;

        ViewTaskTitle.Text = task.Title;
        ViewTaskDescription.Text = string.IsNullOrWhiteSpace(task.Description)
            ? "No description provided."
            : task.Description;

        ViewTaskPriority.Text = $"{task.Priority.ToLower()} priority";

        ViewTaskDue.Text = $"🕒 Due: {shift.Date:MMM d}, {FormatTime(shift.EndTime)}";
        ViewAssignedFrom.Text = "Manager";
        ViewShiftTime.Text = $"{shift.Date:ddd, MMM d}, {FormatTime(shift.StartTime)} - {FormatTime(shift.EndTime)}";
        ViewLocation.Text = shift.Location;
        ViewStatus.Text = shift.Status;

        ViewStatus.TextColor = shift.Status switch
        {
            "Accepted" => Color.FromArgb("#16A34A"),
            "Declined" => Color.FromArgb("#DC2626"),
            _ => Color.FromArgb("#CA8A04")
        };

        TaskDetailsOverlay.IsVisible = true;
    }

    private async void OnMarkAsDoneClicked(object sender, EventArgs e)
    {
        if (_selectedTask == null)
            return;

        _selectedTask.IsCompleted = true;
        await _databaseService.UpdateTaskAsync(_selectedTask);

        TaskDetailsOverlay.IsVisible = false;

        await LoadDashboardDataAsync();
    }

    private void OnCloseTaskDetailsClicked(object sender, EventArgs e)
    {
        TaskDetailsOverlay.IsVisible = false;
    }

    private async void OnNotificationTapped(object sender, TappedEventArgs e)
    {
        Page? page = Window?.Page ?? Application.Current?.Windows[0].Page;

        if (page != null)
        {
            await page.Navigation.PushModalAsync(new EmployeeNotification());
        }
    }

    private View CreateEmptyCard(string title, string subtitle, double padding)
    {
        var stack = new VerticalStackLayout
        {
            Spacing = 8,
            HorizontalOptions = LayoutOptions.Center,
            Children =
            {
                new Label
                {
                    Text = title,
                    FontSize = 14,
                    TextColor = Color.FromArgb("#6B7280"),
                    HorizontalTextAlignment = TextAlignment.Center
                }
            }
        };

        if (!string.IsNullOrWhiteSpace(subtitle))
        {
            stack.Children.Add(new Label
            {
                Text = subtitle,
                FontSize = 12,
                TextColor = Color.FromArgb("#9CA3AF"),
                HorizontalTextAlignment = TextAlignment.Center
            });
        }

        return new Border
        {
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#E5E7EB"),
            Padding = padding,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Content = stack
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

    private View CreatePriorityBadge(string priority)
    {
        var badge = new Border
        {
            Padding = new Thickness(10, 5),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 14 },
            BackgroundColor = priority switch
            {
                "High" => Color.FromArgb("#FEE2E2"),
                "Medium" => Color.FromArgb("#FEF3C7"),
                "Low" => Color.FromArgb("#DBEAFE"),
                _ => Color.FromArgb("#F3F4F6")
            },
            Content = new Label
            {
                Text = priority.ToLower(),
                FontSize = 11,
                TextColor = priority switch
                {
                    "High" => Color.FromArgb("#DC2626"),
                    "Medium" => Color.FromArgb("#B45309"),
                    "Low" => Color.FromArgb("#1E40AF"),
                    _ => Color.FromArgb("#6B7280")
                }
            }
        };

        Grid.SetColumn(badge, 1);
        return badge;
    }

    private static DateTime GetStartOfWeek(DateTime date)
    {
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Sunday)) % 7;
        return date.AddDays(-diff).Date;
    }

    private static string FormatTime(TimeSpan time)
    {
        return DateTime.Today
            .Add(time)
            .ToString("h:mm tt");
    }
}