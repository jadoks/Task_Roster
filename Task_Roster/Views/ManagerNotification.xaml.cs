using Task_Roster.Models;
using Task_Roster.Services;

namespace Task_Roster.Views;

public partial class ManagerNotification : ContentPage
{
    private readonly DatabaseService _databaseService;

    public ManagerNotification()
    {
        InitializeComponent();

        _databaseService = new DatabaseService();

        LoadNotifications();
    }

    private async void LoadNotifications()
    {
        var notifications = new List<ManagerNotificationItem>();

        var shifts = await _databaseService.GetShiftsAsync();

        foreach (var shift in shifts.OrderByDescending(s => s.Date).ThenByDescending(s => s.StartTime))
        {
            if (shift.Status == "Pending")
            {
                notifications.Add(new ManagerNotificationItem
                {
                    Title = "Pending Shift",
                    Message = $"{shift.Role} shift at {shift.Location} on {shift.Date:dddd}, {FormatTime(shift.StartTime)} - {FormatTime(shift.EndTime)} is still pending.",
                    Date = shift.Date.Date.Add(shift.StartTime),
                    IconSource = "schedule.svg",
                    IconBackgroundColor = Color.FromArgb("#FEF3C7")
                });
            }

            if (shift.Employee == "-- Unassigned --" || string.IsNullOrWhiteSpace(shift.Employee))
            {
                notifications.Add(new ManagerNotificationItem
                {
                    Title = "Unassigned Shift",
                    Message = $"{shift.Role} shift at {shift.Location} on {shift.Date:dddd} has no assigned employee.",
                    Date = shift.Date.Date.Add(shift.StartTime),
                    IconSource = "exclamation.svg",
                    IconBackgroundColor = Color.FromArgb("#FEE2E2")
                });
            }

            var tasks = await _databaseService.GetTasksByShiftIdAsync(shift.Id);

            foreach (var task in tasks.OrderByDescending(t => t.CreatedAt))
            {
                notifications.Add(new ManagerNotificationItem
                {
                    Title = task.IsCompleted ? "Task Completed" : "Task Pending",
                    Message = task.IsCompleted
                        ? $"{shift.Employee} completed \"{task.Title}\" for the {shift.Role} shift."
                        : $"Task \"{task.Title}\" for {shift.Employee} is still pending.",
                    Date = task.CreatedAt,
                    IconSource = task.IsCompleted ? "check.svg" : "clipboard.svg",
                    IconBackgroundColor = task.IsCompleted
                        ? Color.FromArgb("#DCFCE7")
                        : Color.FromArgb("#DBEAFE")
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

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private static void ApplyDateHeaders(List<ManagerNotificationItem> notifications)
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

    private static string FormatTime(TimeSpan time)
    {
        return DateTime.Today.Add(time).ToString("h:mm tt");
    }
}

public class ManagerNotificationItem
{
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public DateTime Date { get; set; }
    public string IconSource { get; set; } = "";
    public Color IconBackgroundColor { get; set; } = Colors.White;
    public bool ShowDateHeader { get; set; }
    public string DateLabel { get; set; } = "";
    public string TimeLabel => Date.ToString("h:mm tt");
}
