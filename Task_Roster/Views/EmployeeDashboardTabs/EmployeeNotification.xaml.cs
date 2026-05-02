using Task_Roster.Models;
using Task_Roster.Services;

namespace Task_Roster.Views.EmployeeDashboardTabs;

public partial class EmployeeNotification : ContentPage
{
    private readonly DatabaseService _databaseService;

    public EmployeeNotification()
    {
        InitializeComponent();

        _databaseService = new DatabaseService();

        LoadNotifications();
    }

    private async void LoadNotifications()
    {
        string currentEmail = Preferences.Get("UserEmail", "");
        string currentName = Preferences.Get("UserName", "");

        UserModel? currentUser = null;

        if (!string.IsNullOrWhiteSpace(currentEmail))
        {
            currentUser = await _databaseService.GetUserByEmailAsync(currentEmail);
        }

        string fullName = "";

        if (currentUser != null)
        {
            fullName = $"{currentUser.FirstName} {currentUser.LastName}".Trim();
            currentEmail = currentUser.Email;
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            fullName = currentName;
        }

        var notifications = new List<EmployeeNotificationItem>();

        var shifts = await _databaseService.GetShiftsAsync();

        var assignedShifts = shifts
            .Where(s =>
                IsCurrentEmployee(s.Employee, fullName, currentEmail))
            .OrderByDescending(s => s.Date)
            .ThenByDescending(s => s.StartTime)
            .ToList();

        foreach (var shift in assignedShifts)
        {
            notifications.Add(new EmployeeNotificationItem
            {
                Title = "New Shift Assigned",
                Message = $"You have been assigned a {shift.Role} shift at {shift.Location} on {shift.Date:dddd} from {FormatTime(shift.StartTime)} to {FormatTime(shift.EndTime)}.",
                Date = shift.Date.Date.Add(shift.StartTime),
                Type = "Shift",
                IconSource = "calendar.svg",
                IconBackgroundColor = Color.FromArgb("#DBEAFE")
            });

            var tasks = await _databaseService.GetTasksByShiftIdAsync(shift.Id);

            foreach (var task in tasks.OrderByDescending(t => t.CreatedAt))
            {
                notifications.Add(new EmployeeNotificationItem
                {
                    Title = task.IsCompleted ? "Task Completed" : "Task Reminder",
                    Message = task.IsCompleted
                        ? $"Task \"{task.Title}\" for your {shift.Role} shift has been marked completed."
                        : $"Task \"{task.Title}\" is pending for your {shift.Role} shift at {shift.Location}.",
                    Date = task.CreatedAt,
                    Type = "Task",
                    IconSource = "clipboard.svg",
                    IconBackgroundColor = task.IsCompleted
                        ? Color.FromArgb("#DCFCE7")
                        : Color.FromArgb("#FEF3C7")
                });
            }
        }

        var ordered = notifications
            .OrderByDescending(n => n.Date)
            .ToList();

        ApplyDateHeaders(ordered);

        NotificationsList.ItemsSource = ordered;

        EmptyState.IsVisible = ordered.Count == 0;
        NotificationsList.IsVisible = ordered.Count > 0;
    }

    private static bool IsCurrentEmployee(string assignedEmployee, string fullName, string email)
    {
        if (string.IsNullOrWhiteSpace(assignedEmployee))
            return false;

        if (assignedEmployee == "-- Unassigned --")
            return false;

        return assignedEmployee.Equals(fullName, StringComparison.OrdinalIgnoreCase) ||
               assignedEmployee.Equals(email, StringComparison.OrdinalIgnoreCase);
    }

    private static void ApplyDateHeaders(List<EmployeeNotificationItem> notifications)
    {
        string previousDate = "";

        foreach (var item in notifications)
        {
            string currentDate = item.Date.ToString("MMMM d, yyyy");

            item.ShowDateHeader = currentDate != previousDate;
            item.DateLabel = currentDate;

            previousDate = currentDate;
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private static string FormatTime(TimeSpan time)
    {
        return DateTime.Today
            .Add(time)
            .ToString("h:mm tt");
    }
}

public class EmployeeNotificationItem
{
    public string Title { get; set; } = "";

    public string Message { get; set; } = "";

    public DateTime Date { get; set; }

    public string Type { get; set; } = "";

    public string IconSource { get; set; } = "";

    public Color IconBackgroundColor { get; set; } = Colors.White;

    public bool ShowDateHeader { get; set; }

    public string DateLabel { get; set; } = "";

    public string TimeLabel => Date.ToString("h:mm tt");
}