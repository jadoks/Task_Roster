using System.Globalization;
using Microsoft.Maui.Controls.Shapes;
namespace Task_Roster.Views.DashboardTabs;

public partial class ScheduleView : ContentView
{
    private DateTime _weekStart;
    private ShiftModel? _pendingShift;
    private ShiftModel? _selectedShift;

    public ScheduleView()
    {
        InitializeComponent();

        _weekStart = GetStartOfWeek(DateTime.Today);

        RolePicker.SelectedIndex = 0;
        LocationPicker.SelectedIndex = 0;
        EmployeePicker.SelectedIndex = 0;
        ShiftDatePicker.Date = DateTime.Today;

        RenderWeek();
    }

    private void RenderWeek()
    {
        WeekRangeLabel.Text = $"{_weekStart:MMM d}  -  {_weekStart.AddDays(6):MMM d}";

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
                Text = day.ToString("ddd", CultureInfo.InvariantCulture),
                FontSize = 10,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Black,
                HorizontalTextAlignment = TextAlignment.Center
            });

            dayStack.Children.Add(new Label
            {
                Text = day.ToString("MMM d", CultureInfo.InvariantCulture),
                FontSize = 9,
                TextColor = Color.FromArgb("#6B7280"),
                HorizontalTextAlignment = TextAlignment.Center
            });

            DaysHeaderGrid.Add(dayStack, i, 0);

            var addButton = new Border
            {
                BackgroundColor = Colors.White,
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
                            Text = "+",
                            FontSize = 22,
                            TextColor = Color.FromArgb("#6B7280"),
                            HorizontalTextAlignment = TextAlignment.Center
                        },
                        new Label
                        {
                            Text = "Add",
                            FontSize = 10,
                            TextColor = Color.FromArgb("#6B7280"),
                            HorizontalTextAlignment = TextAlignment.Center
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

    private void OnAddShiftClicked(object sender, EventArgs e)
    {
        OpenAddShiftModal();
    }

    private void OpenAddShiftModal()
    {
        AddShiftOverlay.IsVisible = true;
    }

    private void OnCancelAddShiftClicked(object sender, EventArgs e)
    {
        AddShiftOverlay.IsVisible = false;
    }

    private void OnCreateShiftClicked(object sender, EventArgs e)
    {
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

    private void OnConfirmCreateClicked(object sender, EventArgs e)
    {
        if (_pendingShift == null)
            return;

        _selectedShift = _pendingShift;
        ConfirmOverlay.IsVisible = false;

        AddShiftCard(_selectedShift);
        ShowShiftDetails(_selectedShift);

        _pendingShift = null;
    }

    private void AddShiftCard(ShiftModel shift)
    {
        int column = (shift.Date.Date - _weekStart.Date).Days;

        if (column < 0 || column > 6)
        {
            _weekStart = GetStartOfWeek(shift.Date);
            RenderWeek();
            column = (shift.Date.Date - _weekStart.Date).Days;
        }

        var card = new Border
        {
            BackgroundColor = Color.FromArgb("#DCFCE7"),
            Stroke = Color.FromArgb("#22C55E"),
            StrokeThickness = 2,
            Padding = 6,
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
                    }
                }
            }
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => ShowShiftDetails(shift);
        card.GestureRecognizers.Add(tap);

        ScheduleGrid.Children.RemoveAt(column);
        ScheduleGrid.Add(card, column, 0);
    }

    private void ShowShiftDetails(ShiftModel shift)
    {
        _selectedShift = shift;

        DetailsRoleLabel.Text = shift.Role.ToLower();
        DetailsStatusLabel.Text = shift.Status;
        DetailsDateLabel.Text = $"📅  {shift.Date:dddd, MMMM d, yyyy}";
        DetailsTimeLabel.Text = $"🕘  {FormatTime(shift.StartTime)} - {FormatTime(shift.EndTime)}";
        DetailsLocationLabel.Text = $"📍  {shift.Location}";
        DetailsEmployeeLabel.Text = shift.Employee == "-- Unassigned --" ? "Unassigned" : shift.Employee;

        ShiftDetailsCard.IsVisible = true;
    }

    private async void OnAutoAssignClicked(object sender, EventArgs e)
    {
        EmployeePicker.SelectedIndex = 1;
        await Application.Current.MainPage.DisplayAlert("Auto Assign", "Employee automatically assigned.", "OK");
    }

    private async void OnAddTaskClicked(object sender, EventArgs e)
    {
        await Application.Current.MainPage.DisplayAlert("Task", "Add Task clicked.", "OK");
    }

    private void OnTaskCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        TaskCountLabel.Text = e.Value ? "1/1 completed" : "0/1 completed";
    }

    private void OnAcceptClicked(object sender, EventArgs e)
    {
        if (_selectedShift == null)
            return;

        _selectedShift.Status = "Accepted";
        DetailsStatusLabel.Text = "Accepted";
        DetailsStatusLabel.TextColor = Color.FromArgb("#16A34A");
    }

    private void OnDeclineClicked(object sender, EventArgs e)
    {
        if (_selectedShift == null)
            return;

        _selectedShift.Status = "Declined";
        DetailsStatusLabel.Text = "Declined";
        DetailsStatusLabel.TextColor = Color.FromArgb("#DC2626");
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

    private class ShiftModel
    {
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Role { get; set; } = "";
        public string Location { get; set; } = "";
        public string Employee { get; set; } = "";
        public string Status { get; set; } = "Pending";
    }
}