using System.Globalization;
using Microsoft.Maui.Controls.Shapes;
using Task_Roster.Models;
using Task_Roster.Services;

namespace Task_Roster.Views.DashboardTabs;

public partial class ScheduleView : ContentView
{
    private readonly DatabaseService _databaseService;

    private DateTime _weekStart;
    private ShiftModel? _pendingShift;
    private ShiftModel? _selectedShift;
    private TaskModel? _selectedTask;

    private string _selectedTaskPriority = "Medium";

    public ScheduleView()
    {
        InitializeComponent();

        _databaseService = new DatabaseService();
        _weekStart = GetStartOfWeek(DateTime.Today);

        RolePicker.SelectedIndex = 0;
        LocationPicker.SelectedIndex = 0;
        ShiftDatePicker.Date = DateTime.Today;
        ShiftDatePicker.MinimumDate = DateTime.Today;

        LoadEmployees();
        RenderWeek();
    }

    private async void LoadEmployees()
    {
        var employees = await _databaseService.GetEmployeesAsync();

        EmployeePicker.Items.Clear();
        EmployeePicker.Items.Add("-- Unassigned --");

        foreach (var employee in employees)
        {
            EmployeePicker.Items.Add(employee.Name);
        }

        EmployeePicker.SelectedIndex = 0;
    }

    private async Task<EmployeeModel?> FindBestEmployeeAsync(string role, DateTime shiftDate)
    {
        var employees = await _databaseService.GetEmployeesAsync();
        string dayName = shiftDate.DayOfWeek.ToString();

        var qualifiedEmployees = employees
            .Where(e =>
                e.Skills.Contains(role, StringComparison.OrdinalIgnoreCase) &&
                e.Availability.Contains(dayName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (qualifiedEmployees.Count == 0)
            return null;

        Random random = new();
        return qualifiedEmployees[random.Next(qualifiedEmployees.Count)];
    }

    private async void RenderWeek()
    {
        WeekRangeLabel.Text = $"{_weekStart:MMM d}  -  {_weekStart.AddDays(6):MMM d}";

        DaysHeaderGrid.Children.Clear();
        ScheduleGrid.Children.Clear();
        ShiftDetailsCard.IsVisible = false;

        for (int i = 0; i < 7; i++)
        {
            DateTime day = _weekStart.AddDays(i);
            bool isPastDay = day.Date < DateTime.Today;

            var dayStack = new VerticalStackLayout
            {
                Spacing = 4,
                HorizontalOptions = LayoutOptions.Center,
                Children =
                {
                    new Label
                    {
                        Text = day.ToString("ddd", CultureInfo.InvariantCulture),
                        FontSize = 10,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = isPastDay ? Color.FromArgb("#9CA3AF") : Colors.Black,
                        HorizontalTextAlignment = TextAlignment.Center
                    },
                    new Label
                    {
                        Text = day.ToString("MMM d", CultureInfo.InvariantCulture),
                        FontSize = 9,
                        TextColor = isPastDay ? Color.FromArgb("#9CA3AF") : Color.FromArgb("#6B7280"),
                        HorizontalTextAlignment = TextAlignment.Center
                    }
                }
            };

            DaysHeaderGrid.Add(dayStack, i, 0);

            var addButton = new Border
            {
                BackgroundColor = isPastDay ? Color.FromArgb("#F3F4F6") : Colors.White,
                Stroke = Color.FromArgb("#D1D5DB"),
                StrokeDashArray = new DoubleCollection { 3, 3 },
                Padding = 8,
                StrokeShape = new RoundRectangle { CornerRadius = 6 },
                Content = new VerticalStackLayout
                {
                    Spacing = 2,
                    HorizontalOptions = LayoutOptions.Center,
                    Children =
                    {
                        new Label
                        {
                            Text = isPastDay ? "×" : "+",
                            FontSize = 22,
                            TextColor = Color.FromArgb("#6B7280"),
                            HorizontalTextAlignment = TextAlignment.Center
                        },
                        new Label
                        {
                            Text = isPastDay ? "Past" : "Add",
                            FontSize = 10,
                            TextColor = Color.FromArgb("#6B7280"),
                            HorizontalTextAlignment = TextAlignment.Center
                        }
                    }
                }
            };

            var tap = new TapGestureRecognizer();
            tap.Tapped += async (_, _) =>
            {
                if (day.Date < DateTime.Today)
                {
                    await ShowAlert("Invalid Date", "Managers cannot add shifts for past days.");
                    return;
                }

                ShiftDatePicker.Date = day;
                OpenAddShiftModal();
            };

            addButton.GestureRecognizers.Add(tap);
            ScheduleGrid.Add(addButton, i, 0);
        }

        await LoadShiftsFromSQLiteAsync();
    }

    private async Task LoadShiftsFromSQLiteAsync()
    {
        var shifts = await _databaseService.GetShiftsAsync();

        var weekShifts = shifts
            .Where(s =>
                s.Date.Date >= _weekStart.Date &&
                s.Date.Date <= _weekStart.AddDays(6).Date)
            .OrderBy(s => s.Date)
            .ThenBy(s => s.StartTime)
            .ToList();

        foreach (var shift in weekShifts)
        {
            AddShiftCard(shift);
        }
    }

    private void OnPreviousWeekClicked(object sender, EventArgs e)
    {
        _weekStart = _weekStart.AddDays(-7);
        RenderWeek();
    }

    private void OnNextWeekClicked(object sender, EventArgs e)
    {
        _weekStart = _weekStart.AddDays(7);
        RenderWeek();
    }

    private async void OnAddShiftClicked(object sender, EventArgs e)
    {
        if (ShiftDatePicker.Date.Date < DateTime.Today)
        {
            await ShowAlert("Invalid Date", "Managers cannot add shifts for past days.");
            ShiftDatePicker.Date = DateTime.Today;
            return;
        }

        OpenAddShiftModal();
    }

    private void OpenAddShiftModal()
    {
        if (ShiftDatePicker.Date.Date < DateTime.Today)
            ShiftDatePicker.Date = DateTime.Today;

        AddShiftOverlay.IsVisible = true;
    }

    private void OnCancelAddShiftClicked(object sender, EventArgs e)
    {
        AddShiftOverlay.IsVisible = false;
    }

    private async void OnAutoAssignClicked(object sender, EventArgs e)
    {
        if (ShiftDatePicker.Date.Date < DateTime.Today)
        {
            await ShowAlert("Invalid Date", "Managers cannot auto-assign shifts for past days.");
            ShiftDatePicker.Date = DateTime.Today;
            return;
        }

        string role = RolePicker.SelectedItem?.ToString() ?? "Cashier";
        DateTime shiftDate = ShiftDatePicker.Date;

        var employee = await FindBestEmployeeAsync(role, shiftDate);

        if (employee == null)
        {
            await ShowAlert("No Match Found", $"No available employee found for {role}.");
            return;
        }

        EmployeePicker.SelectedItem = employee.Name;
        await ShowAlert("Auto Assign Complete", $"{employee.Name} assigned automatically.");
    }

    private async void OnCreateShiftClicked(object sender, EventArgs e)
    {
        if (ShiftDatePicker.Date.Date < DateTime.Today)
        {
            await ShowAlert("Invalid Date", "Managers cannot create shifts for past days.");
            ShiftDatePicker.Date = DateTime.Today;
            return;
        }

        if (EndTimePicker.Time <= StartTimePicker.Time)
        {
            await ShowAlert("Invalid Time", "End time must be later than start time.");
            return;
        }

        _pendingShift = new ShiftModel
        {
            Date = ShiftDatePicker.Date,
            StartTime = StartTimePicker.Time,
            EndTime = EndTimePicker.Time,
            Role = RolePicker.SelectedItem?.ToString() ?? "Cashier",
            Location = LocationPicker.SelectedItem?.ToString() ?? "Downtown Store",
            Employee = EmployeePicker.SelectedItem?.ToString() ?? "-- Unassigned --",
            Status = "Pending"
        };

        ConfirmDateLabel.Text = $"📅  Date\n{_pendingShift.Date:dddd, MMMM d, yyyy}";
        ConfirmTimeLabel.Text = $"🕘  Time\n{FormatTime(_pendingShift.StartTime)} - {FormatTime(_pendingShift.EndTime)}";
        ConfirmLocationLabel.Text = $"📍  Location\n{_pendingShift.Location}";
        ConfirmRoleLabel.Text = $"👤  Role Needed\n{_pendingShift.Role}";
        ConfirmEmployeeLabel.Text = $"🧑  Assigned To\n{_pendingShift.Employee}";

        AddShiftOverlay.IsVisible = false;
        ConfirmOverlay.IsVisible = true;
    }

    private void OnEditShiftClicked(object sender, EventArgs e)
    {
        ConfirmOverlay.IsVisible = false;
        AddShiftOverlay.IsVisible = true;
    }

    private async void OnConfirmCreateClicked(object sender, EventArgs e)
    {
        if (_pendingShift == null)
            return;

        if (_pendingShift.Date.Date < DateTime.Today)
        {
            await ShowAlert("Invalid Date", "Managers cannot create shifts for past days.");
            ConfirmOverlay.IsVisible = false;
            _pendingShift = null;
            return;
        }

        await _databaseService.AddShiftAsync(_pendingShift);

        _selectedShift = _pendingShift;
        ConfirmOverlay.IsVisible = false;

        RenderWeek();
        ShowShiftDetails(_selectedShift);

        _pendingShift = null;
    }

    private void AddShiftCard(ShiftModel shift)
    {
        int column = (shift.Date.Date - _weekStart.Date).Days;

        if (column < 0 || column > 6)
            return;

        Color bgColor = shift.Status switch
        {
            "Accepted" => Color.FromArgb("#DCFCE7"),
            "Declined" => Color.FromArgb("#FEE2E2"),
            _ => Color.FromArgb("#FEF3C7")
        };

        Color strokeColor = shift.Status switch
        {
            "Accepted" => Color.FromArgb("#22C55E"),
            "Declined" => Color.FromArgb("#DC2626"),
            _ => Color.FromArgb("#F59E0B")
        };

        var card = new Border
        {
            BackgroundColor = bgColor,
            Stroke = strokeColor,
            StrokeThickness = 2,
            Padding = 6,
            Margin = new Thickness(0, 70, 0, 0),
            StrokeShape = new RoundRectangle { CornerRadius = 6 },
            Content = new VerticalStackLayout
            {
                Spacing = 3,
                Children =
                {
                    new Label
                    {
                        Text = FormatTime(shift.StartTime),
                        FontSize = 10,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.Black
                    },
                    new Label
                    {
                        Text = FormatTime(shift.EndTime),
                        FontSize = 10,
                        TextColor = Colors.Black
                    },
                    new Label
                    {
                        Text = shift.Role,
                        FontSize = 10,
                        TextColor = Colors.Black
                    },
                    new Label
                    {
                        Text = string.IsNullOrWhiteSpace(shift.Employee) ? "Unassigned" : shift.Employee,
                        FontSize = 9,
                        TextColor = Color.FromArgb("#166534")
                    }
                }
            }
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => ShowShiftDetails(shift);

        card.GestureRecognizers.Add(tap);
        ScheduleGrid.Add(card, column, 0);
    }

    private async void ShowShiftDetails(ShiftModel shift)
    {
        _selectedShift = shift;
        _selectedTask = null;

        DetailsRoleLabel.Text = shift.Role.ToLower();
        DetailsStatusLabel.Text = shift.Status;

        DetailsStatusLabel.TextColor = shift.Status switch
        {
            "Accepted" => Color.FromArgb("#16A34A"),
            "Declined" => Color.FromArgb("#DC2626"),
            _ => Color.FromArgb("#CA8A04")
        };

        DetailsDateLabel.Text = $"📅  {shift.Date:dddd, MMMM d, yyyy}";
        DetailsTimeLabel.Text = $"🕘  {FormatTime(shift.StartTime)} - {FormatTime(shift.EndTime)}";
        DetailsLocationLabel.Text = $"📍  {shift.Location}";

        await LoadAssignedUserAsync(shift.Employee);

        ShiftDetailsCard.IsVisible = true;

        await LoadShiftTasksAsync(shift);
    }

    private async Task LoadAssignedUserAsync(string employeeName)
    {
        DetailsProfileImage.Source = null;
        DetailsProfileImage.IsVisible = false;
        DetailsProfileFallbackLabel.IsVisible = true;

        if (string.IsNullOrWhiteSpace(employeeName) || employeeName == "-- Unassigned --")
        {
            DetailsEmployeeLabel.Text = "Unassigned";
            DetailsEmployeeEmailLabel.Text = "No email found";
            return;
        }

        var users = await _databaseService.GetUsersAsync();

        var user = users.FirstOrDefault(u =>
        {
            string fullName = $"{u.FirstName} {u.LastName}".Trim();

            return fullName.Equals(employeeName, StringComparison.OrdinalIgnoreCase) ||
                   u.FirstName.Equals(employeeName, StringComparison.OrdinalIgnoreCase) ||
                   u.Email.Equals(employeeName, StringComparison.OrdinalIgnoreCase);
        });

        if (user == null)
        {
            DetailsEmployeeLabel.Text = employeeName;
            DetailsEmployeeEmailLabel.Text = "No email found";
            return;
        }

        string displayName = $"{user.FirstName} {user.LastName}".Trim();

        DetailsEmployeeLabel.Text = string.IsNullOrWhiteSpace(displayName)
            ? user.Email
            : displayName;

        DetailsEmployeeEmailLabel.Text = user.Email;

        if (user.ProfileImageBytes != null && user.ProfileImageBytes.Length > 0)
        {
            DetailsProfileImage.Source = ImageSource.FromStream(() => new MemoryStream(user.ProfileImageBytes));
            DetailsProfileImage.IsVisible = true;
            DetailsProfileFallbackLabel.IsVisible = false;
        }
    }

    private void OnAddTaskClicked(object sender, EventArgs e)
    {
        if (_selectedShift == null)
            return;

        TaskTitleEntry.Text = string.Empty;
        TaskDescriptionEditor.Text = string.Empty;

        UpdatePriorityUI("Medium");

        AddTaskOverlay.IsVisible = true;
    }

    private void OnCancelAddTaskClicked(object sender, EventArgs e)
    {
        AddTaskOverlay.IsVisible = false;
    }

    private void OnPrioritySelectionClicked(object sender, EventArgs e)
    {
        if (sender is Button clickedButton)
            UpdatePriorityUI(clickedButton.Text);
    }

    private void UpdatePriorityUI(string priority)
    {
        _selectedTaskPriority = priority;

        LowPriorityBtn.BackgroundColor = Color.FromArgb("#F3F4F6");
        LowPriorityBtn.TextColor = Color.FromArgb("#374151");

        MediumPriorityBtn.BackgroundColor = Color.FromArgb("#F3F4F6");
        MediumPriorityBtn.TextColor = Color.FromArgb("#374151");

        HighPriorityBtn.BackgroundColor = Color.FromArgb("#F3F4F6");
        HighPriorityBtn.TextColor = Color.FromArgb("#374151");

        if (priority == "Low")
        {
            LowPriorityBtn.BackgroundColor = Color.FromArgb("#DBEAFE");
            LowPriorityBtn.TextColor = Color.FromArgb("#1E40AF");
        }
        else if (priority == "Medium")
        {
            MediumPriorityBtn.BackgroundColor = Color.FromArgb("#FEF3C7");
            MediumPriorityBtn.TextColor = Color.FromArgb("#CA8A04");
        }
        else if (priority == "High")
        {
            HighPriorityBtn.BackgroundColor = Color.FromArgb("#FEE2E2");
            HighPriorityBtn.TextColor = Color.FromArgb("#991B1B");
        }
    }

    private async void OnSubmitTaskClicked(object sender, EventArgs e)
    {
        if (_selectedShift == null)
        {
            await ShowAlert("Error", "Please select a shift first.");
            return;
        }

        if (string.IsNullOrWhiteSpace(TaskTitleEntry.Text))
        {
            await ShowAlert("Error", "Task title is required.");
            return;
        }

        var task = new TaskModel
        {
            ShiftId = _selectedShift.Id,
            Title = TaskTitleEntry.Text.Trim(),
            Description = TaskDescriptionEditor.Text?.Trim() ?? "",
            Priority = _selectedTaskPriority,
            IsCompleted = false,
            CreatedAt = DateTime.Now
        };

        await _databaseService.AddTaskAsync(task);

        AddTaskOverlay.IsVisible = false;

        await LoadShiftTasksAsync(_selectedShift);
    }

    private async Task LoadShiftTasksAsync(ShiftModel shift)
    {
        var tasks = await _databaseService.GetTasksByShiftIdAsync(shift.Id);

        int completed = tasks.Count(t => t.IsCompleted);
        int total = tasks.Count;

        TaskCountLabel.Text = $"{completed}/{total} completed";

        if (tasks.Count > 0)
        {
            var firstTask = tasks
                .OrderBy(t => t.IsCompleted)
                .ThenByDescending(t => t.CreatedAt)
                .First();

            _selectedTask = firstTask;

            TaskTitleLabel.Text = firstTask.Title;
            TaskPriorityLabel.Text = firstTask.Priority.ToLower();

            TaskPriorityBadge.BackgroundColor = firstTask.Priority switch
            {
                "High" => Color.FromArgb("#FEE2E2"),
                "Medium" => Color.FromArgb("#FEF3C7"),
                "Low" => Color.FromArgb("#DBEAFE"),
                _ => Color.FromArgb("#F3F4F6")
            };

            TaskPriorityLabel.TextColor = firstTask.Priority switch
            {
                "High" => Color.FromArgb("#991B1B"),
                "Medium" => Color.FromArgb("#CA8A04"),
                "Low" => Color.FromArgb("#1E40AF"),
                _ => Color.FromArgb("#374151")
            };
        }
        else
        {
            _selectedTask = null;

            TaskTitleLabel.Text = "No tasks yet";
            TaskPriorityLabel.Text = "-";
            TaskPriorityBadge.BackgroundColor = Color.FromArgb("#F3F4F6");
            TaskPriorityLabel.TextColor = Color.FromArgb("#6B7280");

            TaskCountLabel.Text = "0/0 completed";
        }
    }

    private void ShowTaskDetails(TaskModel task)
    {
        _selectedTask = task;

        ViewTaskTitle.Text = task.Title;
        ViewTaskDescription.Text = string.IsNullOrWhiteSpace(task.Description)
            ? "No description provided."
            : task.Description;

        ViewTaskPriority.Text = $"{task.Priority.ToLower()} priority";

        if (_selectedShift != null)
        {
            ViewTaskDue.Text = $"🕒 Due: {_selectedShift.Date:MMM d}, {FormatTime(_selectedShift.EndTime)}";
            ViewAssignedStaff.Text =
                string.IsNullOrWhiteSpace(_selectedShift.Employee)
                    ? "Unassigned"
                    : _selectedShift.Employee;

            ViewShiftTime.Text = $"{_selectedShift.Date:ddd, MMM d}, {FormatTime(_selectedShift.StartTime)} - {FormatTime(_selectedShift.EndTime)}";
            ViewLocation.Text = _selectedShift.Location;
            ViewStatus.Text = _selectedShift.Status;
        }

        TaskDetailsOverlay.IsVisible = true;
    }

    private async void OnMarkAsDoneClicked(object sender, EventArgs e)
    {
        if (_selectedTask == null)
            return;

        _selectedTask.IsCompleted = true;
        await _databaseService.UpdateTaskAsync(_selectedTask);

        TaskDetailsOverlay.IsVisible = false;

        if (_selectedShift != null)
            await LoadShiftTasksAsync(_selectedShift);
    }

    private void OnCloseTaskDetailsClicked(object sender, EventArgs e)
    {
        TaskDetailsOverlay.IsVisible = false;
    }

    private void OnTaskRowTapped(object sender, TappedEventArgs e)
    {
        if (_selectedTask != null)
            ShowTaskDetails(_selectedTask);
    }

    private async void OnAcceptClicked(object sender, EventArgs e)
    {
        if (_selectedShift == null)
            return;

        _selectedShift.Status = "Accepted";

        await _databaseService.UpdateShiftAsync(_selectedShift);

        DetailsStatusLabel.Text = "Accepted";
        DetailsStatusLabel.TextColor = Color.FromArgb("#16A34A");

        RenderWeek();
        ShowShiftDetails(_selectedShift);
    }

    private async void OnDeclineClicked(object sender, EventArgs e)
    {
        if (_selectedShift == null)
            return;

        _selectedShift.Status = "Declined";

        await _databaseService.UpdateShiftAsync(_selectedShift);

        DetailsStatusLabel.Text = "Declined";
        DetailsStatusLabel.TextColor = Color.FromArgb("#DC2626");

        RenderWeek();
        ShowShiftDetails(_selectedShift);
    }

    private async Task ShowAlert(string title, string message)
    {
        Page? page = Window?.Page ?? Application.Current?.Windows[0].Page;

        if (page != null)
            await page.DisplayAlert(title, message, "OK");
    }

    private static DateTime GetStartOfWeek(DateTime date)
    {
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Sunday)) % 7;
        return date.AddDays(-diff).Date;
    }

    private static string FormatTime(TimeSpan time)
    {
        return DateTime.Today.Add(time).ToString("HH:mm");
    }
}