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

    public ScheduleView()
    {
        InitializeComponent();

        _databaseService = new DatabaseService();

        _weekStart = GetStartOfWeek(DateTime.Today);

        RolePicker.SelectedIndex = 0;

        LocationPicker.SelectedIndex = 0;

        ShiftDatePicker.Date = DateTime.Today;

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

    private async Task<EmployeeModel?> FindBestEmployeeAsync(
        string role,
        DateTime shiftDate)
    {
        var employees = await _databaseService.GetEmployeesAsync();

        string dayName = shiftDate.DayOfWeek.ToString();

        var qualifiedEmployees = employees
            .Where(e =>
                e.Skills.Contains(
                    role,
                    StringComparison.OrdinalIgnoreCase)

                &&

                e.Availability.Contains(
                    dayName,
                    StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (qualifiedEmployees.Count == 0)
            return null;

        Random random = new();

        return qualifiedEmployees[
            random.Next(qualifiedEmployees.Count)];
    }

    private async void RenderWeek()
    {
        WeekRangeLabel.Text =
            $"{_weekStart:MMM d}  -  {_weekStart.AddDays(6):MMM d}";

        DaysHeaderGrid.Children.Clear();

        ScheduleGrid.Children.Clear();

        for (int i = 0; i < 7; i++)
        {
            DateTime day = _weekStart.AddDays(i);

            var dayStack = new VerticalStackLayout
            {
                Spacing = 4,
                HorizontalOptions = LayoutOptions.Center
            };

            dayStack.Children.Add(new Label
            {
                Text = day.ToString(
                    "ddd",
                    CultureInfo.InvariantCulture),

                FontSize = 10,

                FontAttributes = FontAttributes.Bold,

                TextColor = Colors.Black,

                HorizontalTextAlignment =
                    TextAlignment.Center
            });

            dayStack.Children.Add(new Label
            {
                Text = day.ToString(
                    "MMM d",
                    CultureInfo.InvariantCulture),

                FontSize = 9,

                TextColor = Color.FromArgb("#6B7280"),

                HorizontalTextAlignment =
                    TextAlignment.Center
            });

            DaysHeaderGrid.Add(dayStack, i, 0);

            var addButton = new Border
            {
                BackgroundColor = Colors.White,

                Stroke = Color.FromArgb("#D1D5DB"),

                StrokeDashArray =
                    new DoubleCollection { 3, 3 },

                Padding = 8,

                StrokeShape = new RoundRectangle
                {
                    CornerRadius = 6
                },

                Content = new VerticalStackLayout
                {
                    Spacing = 2,

                    HorizontalOptions =
                        LayoutOptions.Center,

                    Children =
                    {
                        new Label
                        {
                            Text = "+",

                            FontSize = 22,

                            TextColor =
                                Color.FromArgb("#6B7280"),

                            HorizontalTextAlignment =
                                TextAlignment.Center
                        },

                        new Label
                        {
                            Text = "Add",

                            FontSize = 10,

                            TextColor =
                                Color.FromArgb("#6B7280"),

                            HorizontalTextAlignment =
                                TextAlignment.Center
                        }
                    }
                }
            };

            var tap = new TapGestureRecognizer();

            tap.Tapped += (_, _) =>
            {
                ShiftDatePicker.Date = day;

                OpenAddShiftModal();
            };

            addButton.GestureRecognizers.Add(tap);

            ScheduleGrid.Add(addButton, i, 0);
        }

        await LoadShifts();
    }

    private async Task LoadShifts()
    {
        var shifts = await _databaseService.GetShiftsAsync();

        foreach (var shift in shifts)
        {
            if (shift.Date.Date >= _weekStart.Date
                && shift.Date.Date <= _weekStart.AddDays(6).Date)
            {
                AddShiftCard(shift);
            }
        }
    }

    private void OnPreviousWeekClicked(
        object sender,
        EventArgs e)
    {
        _weekStart = _weekStart.AddDays(-7);

        RenderWeek();
    }

    private void OnNextWeekClicked(
        object sender,
        EventArgs e)
    {
        _weekStart = _weekStart.AddDays(7);

        RenderWeek();
    }

    private void OnAddShiftClicked(
        object sender,
        EventArgs e)
    {
        OpenAddShiftModal();
    }

    private void OpenAddShiftModal()
    {
        AddShiftOverlay.IsVisible = true;
    }

    private void OnCancelAddShiftClicked(
        object sender,
        EventArgs e)
    {
        AddShiftOverlay.IsVisible = false;
    }

    private async void OnAutoAssignClicked(
        object sender,
        EventArgs e)
    {
        string role =
            RolePicker.SelectedItem?.ToString()
            ?? "Cashier";

        DateTime shiftDate = ShiftDatePicker.Date;

        var employee =
            await FindBestEmployeeAsync(
                role,
                shiftDate);

        if (employee == null)
        {
            await Application.Current.MainPage.DisplayAlert(
                "No Match Found",
                $"No available employee found for {role}.",
                "OK");

            return;
        }

        EmployeePicker.SelectedItem = employee.Name;

        await Application.Current.MainPage.DisplayAlert(
            "Auto Assign Complete",
            $"{employee.Name} assigned automatically.",
            "OK");
    }

    private void OnCreateShiftClicked(
        object sender,
        EventArgs e)
    {
        _pendingShift = new ShiftModel
        {
            Date = ShiftDatePicker.Date,

            StartTime = StartTimePicker.Time,

            EndTime = EndTimePicker.Time,

            Role = RolePicker.SelectedItem?.ToString()
                   ?? "Cashier",

            Location = LocationPicker.SelectedItem?.ToString()
                       ?? "Downtown Store",

            Employee = EmployeePicker.SelectedItem?.ToString()
                       ?? "-- Unassigned --",

            Status = "Pending"
        };

        ConfirmDateLabel.Text =
            $"📅  Date\n{_pendingShift.Date:dddd, MMMM d, yyyy}";

        ConfirmTimeLabel.Text =
            $"🕘  Time\n{FormatTime(_pendingShift.StartTime)} - {FormatTime(_pendingShift.EndTime)}";

        ConfirmLocationLabel.Text =
            $"📍  Location\n{_pendingShift.Location}";

        ConfirmRoleLabel.Text =
            $"👤  Role Needed\n{_pendingShift.Role}";

        ConfirmEmployeeLabel.Text =
            $"🧑  Assigned To\n{_pendingShift.Employee}";

        AddShiftOverlay.IsVisible = false;

        ConfirmOverlay.IsVisible = true;
    }

    private void OnEditShiftClicked(
        object sender,
        EventArgs e)
    {
        ConfirmOverlay.IsVisible = false;

        AddShiftOverlay.IsVisible = true;
    }

    private async void OnConfirmCreateClicked(
        object sender,
        EventArgs e)
    {
        if (_pendingShift == null)
            return;

        await _databaseService.AddShiftAsync(
            _pendingShift);

        _selectedShift = _pendingShift;

        ConfirmOverlay.IsVisible = false;

        AddShiftCard(_selectedShift);

        ShowShiftDetails(_selectedShift);

        _pendingShift = null;
    }

    private void AddShiftCard(ShiftModel shift)
    {
        int column =
            (shift.Date.Date - _weekStart.Date).Days;

        if (column < 0 || column > 6)
            return;

        var card = new Border
        {
            BackgroundColor =
                Color.FromArgb("#DCFCE7"),

            Stroke =
                Color.FromArgb("#22C55E"),

            StrokeThickness = 2,

            Padding = 6,

            Margin = new Thickness(0, 70, 0, 0),

            StrokeShape = new RoundRectangle
            {
                CornerRadius = 6
            },

            Content = new VerticalStackLayout
            {
                Spacing = 3,

                Children =
                {
                    new Label
                    {
                        Text =
                            FormatTime(
                                shift.StartTime),

                        FontSize = 10,

                        FontAttributes =
                            FontAttributes.Bold,

                        TextColor = Colors.Black
                    },

                    new Label
                    {
                        Text =
                            FormatTime(
                                shift.EndTime),

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
                        Text = shift.Employee,

                        FontSize = 9,

                        TextColor =
                            Color.FromArgb("#166534")
                    }
                }
            }
        };

        var tap = new TapGestureRecognizer();

        tap.Tapped += (_, _) =>
        {
            ShowShiftDetails(shift);
        };

        card.GestureRecognizers.Add(tap);

        ScheduleGrid.Add(card, column, 0);
    }

    private void ShowShiftDetails(ShiftModel shift)
    {
        _selectedShift = shift;

        DetailsRoleLabel.Text =
            shift.Role.ToLower();

        DetailsStatusLabel.Text =
            shift.Status;

        DetailsDateLabel.Text =
            $"📅  {shift.Date:dddd, MMMM d, yyyy}";

        DetailsTimeLabel.Text =
            $"🕘  {FormatTime(shift.StartTime)} - {FormatTime(shift.EndTime)}";

        DetailsLocationLabel.Text =
            $"📍  {shift.Location}";

        DetailsEmployeeLabel.Text =
            shift.Employee == "-- Unassigned --"
                ? "Unassigned"
                : shift.Employee;

        ShiftDetailsCard.IsVisible = true;
    }

    private string _selectedPriority = "Medium";

    private void OnAddTaskClicked(object sender, EventArgs e)
    {
        // Clear fields before showing
        TaskTitleEntry.Text = string.Empty;
        TaskDescriptionEditor.Text = string.Empty;

        // Reset priority to Medium by default
        UpdatePriorityUI("Medium");

        AddTaskOverlay.IsVisible = true;
    }

    private void OnCancelAddTaskClicked(object sender, EventArgs e)
    {
        AddTaskOverlay.IsVisible = false;
    }
    private string _selectedTaskPriority = "Medium";

    private void OnPrioritySelectionClicked(object sender, EventArgs e)
    {
        if (sender is Button clickedButton)
        {
            _selectedTaskPriority = clickedButton.Text;

            // Reset all buttons to the default "unselected" look
            LowPriorityBtn.BackgroundColor = Color.FromArgb("#F3F4F6");
            LowPriorityBtn.TextColor = Color.FromArgb("#374151");

            MediumPriorityBtn.BackgroundColor = Color.FromArgb("#F3F4F6");
            MediumPriorityBtn.TextColor = Color.FromArgb("#374151");

            HighPriorityBtn.BackgroundColor = Color.FromArgb("#F3F4F6");
            HighPriorityBtn.TextColor = Color.FromArgb("#374151");

            // Apply "selected" colors based on which one was clicked
            if (_selectedTaskPriority == "Low")
            {
                LowPriorityBtn.BackgroundColor = Color.FromArgb("#DBEAFE");
                LowPriorityBtn.TextColor = Color.FromArgb("#1E40AF");
            }
            else if (_selectedTaskPriority == "Medium")
            {
                MediumPriorityBtn.BackgroundColor = Color.FromArgb("#FEF3C7");
                MediumPriorityBtn.TextColor = Color.FromArgb("#CA8A04");
            }
            else if (_selectedTaskPriority == "High")
            {
                HighPriorityBtn.BackgroundColor = Color.FromArgb("#FEE2E2");
                HighPriorityBtn.TextColor = Color.FromArgb("#991B1B");
            }
        }
    }
    private void UpdatePriorityUI(string priority)
    {
        _selectedPriority = priority;

        // Reset all to default style
        LowPriorityBtn.BackgroundColor = HighPriorityBtn.BackgroundColor = Color.FromArgb("#F9FAFB");
        LowPriorityBtn.TextColor = HighPriorityBtn.TextColor = Color.FromArgb("#4B5563");
        LowPriorityBtn.BorderWidth = HighPriorityBtn.BorderWidth = 0;

        MediumPriorityBtn.BackgroundColor = Color.FromArgb("#F9FAFB");
        MediumPriorityBtn.TextColor = Color.FromArgb("#4B5563");
        MediumPriorityBtn.BorderWidth = 0;

        // Apply "Selected" style based on choice
        if (priority == "Low")
        {
            LowPriorityBtn.BackgroundColor = Color.FromArgb("#F3F4F6");
            LowPriorityBtn.BorderColor = Color.FromArgb("#D1D5DB");
            LowPriorityBtn.BorderWidth = 1;
        }
        else if (priority == "Medium")
        {
            MediumPriorityBtn.BackgroundColor = Color.FromArgb("#FEF9C3");
            MediumPriorityBtn.TextColor = Color.FromArgb("#854D0E");
            MediumPriorityBtn.BorderColor = Color.FromArgb("#FDE047");
            MediumPriorityBtn.BorderWidth = 1;
        }
        else if (priority == "High")
        {
            HighPriorityBtn.BackgroundColor = Color.FromArgb("#FEE2E2");
            HighPriorityBtn.TextColor = Color.FromArgb("#991B1B");
            HighPriorityBtn.BorderColor = Color.FromArgb("#FCA5A5");
            HighPriorityBtn.BorderWidth = 1;
        }
    }

    // Update your existing Submit method to show the details screen
    private async void OnSubmitTaskClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TaskTitleEntry.Text))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Task title is required", "OK");
            return;
        }

        // 1. Fill the Task Details view with the data just entered
        ViewTaskTitle.Text = TaskTitleEntry.Text;
        ViewTaskDescription.Text = TaskDescriptionEditor.Text;
        ViewTaskPriority.Text = $"{_selectedTaskPriority.ToLower()} priority";

        // 2. Pull data from the currently selected shift (_selectedShift)
        if (_selectedShift != null)
        {
            ViewTaskDue.Text = $"🕒 Due: {_selectedShift.Date:MMM d}, {FormatTime(_selectedShift.EndTime)}";
            ViewAssignedStaff.Text = _selectedShift.Employee;
            ViewShiftTime.Text = $"{_selectedShift.Date:ddd, MMM d} , {FormatTime(_selectedShift.StartTime)} - {FormatTime(_selectedShift.EndTime)}";
            ViewLocation.Text = _selectedShift.Location;
            ViewStatus.Text = _selectedShift.Status;
        }

        // 3. Close the Add Task Modal and open Task Details
        AddTaskOverlay.IsVisible = false;
        TaskDetailsOverlay.IsVisible = true;
    }

    // Handler for the Mark as Done button
    private void OnMarkAsDoneClicked(object sender, EventArgs e)
    {
        // Close the details view and return to the main schedule interface
        TaskDetailsOverlay.IsVisible = false;
        ShiftDetailsCard.IsVisible = false; // Optional: hide the shift card too
    }

    // Back button handler
    private void OnCloseTaskDetailsClicked(object sender, EventArgs e)
    {
        TaskDetailsOverlay.IsVisible = false;
    }

    private void OnTaskCheckedChanged(
        object sender,
        CheckedChangedEventArgs e)
    {
        TaskCountLabel.Text =
            e.Value
                ? "1/1 completed"
                : "0/1 completed";
    }

    private async void OnAcceptClicked(
        object sender,
        EventArgs e)
    {
        if (_selectedShift == null)
            return;

        _selectedShift.Status = "Accepted";

        await _databaseService.UpdateShiftAsync(
            _selectedShift);

        DetailsStatusLabel.Text = "Accepted";

        DetailsStatusLabel.TextColor =
            Color.FromArgb("#16A34A");
    }

    private async void OnDeclineClicked(
        object sender,
        EventArgs e)
        { 
        if (_selectedShift == null)
            return;

        _selectedShift.Status = "Declined";

        await _databaseService.UpdateShiftAsync(
            _selectedShift);

        DetailsStatusLabel.Text = "Declined";

        DetailsStatusLabel.TextColor =
            Color.FromArgb("#DC2626");
    }

    private static DateTime GetStartOfWeek(
        DateTime date)
    {
        int diff =
            (7 + (date.DayOfWeek - DayOfWeek.Sunday))
            % 7;

        return date.AddDays(-diff).Date;
    }

    private static string FormatTime(
        TimeSpan time)
    {
        return DateTime.Today
            .Add(time)
            .ToString("HH:mm");
    }
}