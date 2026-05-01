using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;

namespace Task_Roster.Views.DashboardTabs;

public partial class TeamView : ContentView
{
    private readonly List<EmployeeModel> _employees = new();
    private readonly List<string> _pendingSkills = new();
    private EmployeeModel? _pendingEmployee;

    public TeamView()
    {
        InitializeComponent();

        _employees.Add(new EmployeeModel
        {
            Name = "John Smith",
            Email = "john@example.com",
            Skills = new List<string> { "cashier", "customer service" },
            MaxHours = 24,
            Availability = new List<string> { "W", "F" }
        });

        _employees.Add(new EmployeeModel
        {
            Name = "Sarah Miller",
            Email = "sarah@example.com",
            Skills = new List<string> { "barista", "food prep", "cashier" },
            MaxHours = 32,
            Availability = new List<string> { "T", "S" }
        });

        _employees.Add(new EmployeeModel
        {
            Name = "David Lee",
            Email = "david@example.com",
            Skills = new List<string> { "stock", "cashier" },
            MaxHours = 40,
            Availability = new List<string> { "M", "W", "F" }
        });

        UpdateDayTimeVisibility();
        RenderEmployees();
    }

    private void OnDayCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        UpdateDayTimeVisibility();
    }

    private void UpdateDayTimeVisibility()
    {
        MondayTimeGrid.IsVisible = MondayCheckBox.IsChecked;
        TuesdayTimeGrid.IsVisible = TuesdayCheckBox.IsChecked;
        WednesdayTimeGrid.IsVisible = WednesdayCheckBox.IsChecked;
        ThursdayTimeGrid.IsVisible = ThursdayCheckBox.IsChecked;
        FridayTimeGrid.IsVisible = FridayCheckBox.IsChecked;
        SaturdayTimeGrid.IsVisible = SaturdayCheckBox.IsChecked;
        SundayTimeGrid.IsVisible = SundayCheckBox.IsChecked;
    }

    private void RenderEmployees(string filter = "")
    {
        TeamList.Children.Clear();

        var filtered = _employees
            .Where(e => string.IsNullOrWhiteSpace(filter)
                     || e.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)
                     || e.Email.Contains(filter, StringComparison.OrdinalIgnoreCase))
            .ToList();

        TeamCountLabel.Text = $"Team Members ( {filtered.Count} )";

        foreach (var employee in filtered)
        {
            TeamList.Children.Add(CreateEmployeeCard(employee));
        }
    }

    private View CreateEmployeeCard(EmployeeModel employee)
    {
        var skillLayout = new FlexLayout
        {
            Wrap = FlexWrap.Wrap,
            Direction = FlexDirection.Row
        };

        foreach (var skill in employee.Skills)
        {
            skillLayout.Children.Add(CreatePill(skill, "#BBF7D0", "#166534"));
        }

        var availabilityLayout = new Grid
        {
            ColumnSpacing = 5,
            ColumnDefinitions =
            {
                new ColumnDefinition(),
                new ColumnDefinition(),
                new ColumnDefinition(),
                new ColumnDefinition(),
                new ColumnDefinition(),
                new ColumnDefinition(),
                new ColumnDefinition()
            }
        };

        string[] days = { "S", "M", "T", "W", "T", "F", "S" };

        for (int i = 0; i < days.Length; i++)
        {
            bool active = employee.Availability.Any(a => a.StartsWith(days[i]));

            var dayBox = new Border
            {
                BackgroundColor = Color.FromArgb(active ? "#DCFCE7" : "#F3F4F6"),
                StrokeThickness = 0,
                Padding = 5,
                StrokeShape = new RoundRectangle { CornerRadius = 5 },
                Content = new Label
                {
                    Text = days[i],
                    FontSize = 9,
                    TextColor = Color.FromArgb(active ? "#16A34A" : "#9CA3AF"),
                    HorizontalTextAlignment = TextAlignment.Center
                }
            };

            availabilityLayout.Add(dayBox, i, 0);
        }

        var avatar = new Border
        {
            WidthRequest = 42,
            HeightRequest = 42,
            BackgroundColor = Color.FromArgb("#BBF7D0"),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 21 },
            Content = new Label
            {
                Text = string.IsNullOrWhiteSpace(employee.Name)
                    ? "?"
                    : employee.Name.Substring(0, 1).ToLower(),
                TextColor = Color.FromArgb("#166534"),
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            }
        };

        var infoStack = new VerticalStackLayout
        {
            Margin = new Thickness(10, 0, 0, 0),
            Spacing = 2,
            Children =
            {
                new Label
                {
                    Text = employee.Name,
                    FontSize = 13,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#111827")
                },
                new Label
                {
                    Text = employee.Email,
                    FontSize = 11,
                    TextColor = Color.FromArgb("#6B7280")
                }
            }
        };

        Grid.SetColumn(infoStack, 1);

        var editButton = new Button
        {
            Text = "Edit",
            FontSize = 10,
            BackgroundColor = Colors.Transparent,
            TextColor = Color.FromArgb("#16A34A"),
            Padding = 0,
            VerticalOptions = LayoutOptions.Center
        };

        Grid.SetColumn(editButton, 2);

        var headerGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition(),
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        headerGrid.Children.Add(avatar);
        headerGrid.Children.Add(infoStack);
        headerGrid.Children.Add(editButton);

        var cardContent = new VerticalStackLayout
        {
            Spacing = 10,
            Children =
            {
                headerGrid,
                new BoxView { HeightRequest = 1, Color = Color.FromArgb("#F3F4F6") },
                new Label { Text = "Skills:", FontSize = 10, TextColor = Color.FromArgb("#6B7280") },
                skillLayout,
                new BoxView { HeightRequest = 1, Color = Color.FromArgb("#F3F4F6") },
                new Label { Text = "Availability:", FontSize = 10, TextColor = Color.FromArgb("#6B7280") },
                availabilityLayout,
                new Label
                {
                    Text = $"Max hours:   {employee.MaxHours}h/week",
                    FontSize = 11,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#111827")
                }
            }
        };

        return new Border
        {
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#E5E7EB"),
            Padding = 12,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Content = cardContent
        };
    }

    private View CreatePill(string text, string bg, string fg)
    {
        return new Border
        {
            BackgroundColor = Color.FromArgb(bg),
            StrokeThickness = 0,
            Padding = new Thickness(8, 4),
            Margin = new Thickness(0, 0, 5, 5),
            StrokeShape = new RoundRectangle { CornerRadius = 10 },
            Content = new Label
            {
                Text = text,
                FontSize = 9,
                TextColor = Color.FromArgb(fg)
            }
        };
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        RenderEmployees(e.NewTextValue ?? "");
    }

    private void OnAddEmployeeClicked(object sender, EventArgs e)
    {
        AddEmployeeOverlay.IsVisible = true;
    }

    private void OnCloseAddEmployeeClicked(object sender, EventArgs e)
    {
        AddEmployeeOverlay.IsVisible = false;
    }

    private void OnAddSkillClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(SkillEntry.Text))
            return;

        string skill = SkillEntry.Text.Trim();

        _pendingSkills.Add(skill);
        SelectedSkillsLayout.Children.Add(CreatePill(skill, "#BBF7D0", "#166534"));

        SkillEntry.Text = "";
    }

    private async void OnContinueEmployeeClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            await ShowAlert("Missing Name", "Please enter the employee name.");
            return;
        }

        string name = NameEntry.Text.Trim();

        string email = string.IsNullOrWhiteSpace(EmailEntry.Text)
            ? $"{name.Replace(" ", "").ToLower()}@gmail.com"
            : EmailEntry.Text.Trim();

        int.TryParse(MaxHoursEntry.Text, out int maxHours);

        _pendingEmployee = new EmployeeModel
        {
            Name = name,
            Email = email,
            Skills = _pendingSkills.ToList(),
            MaxHours = maxHours <= 0 ? 40 : maxHours,
            Availability = GetSelectedDaysWithTimes()
        };

        ConfirmNameLabel.Text = _pendingEmployee.Name;
        ConfirmEmailLabel.Text = _pendingEmployee.Email;
        ConfirmHoursLabel.Text = $"Max Hours:   {_pendingEmployee.MaxHours}h/week";

        ConfirmSkillsLayout.Children.Clear();

        foreach (var skill in _pendingEmployee.Skills)
        {
            ConfirmSkillsLayout.Children.Add(CreatePill(skill, "#BBF7D0", "#166534"));
        }

        ConfirmAvailabilityLabel.Text = string.Join(Environment.NewLine, _pendingEmployee.Availability);

        AddEmployeeOverlay.IsVisible = false;
        ConfirmEmployeeOverlay.IsVisible = true;
    }

    private void OnBackToEditClicked(object sender, EventArgs e)
    {
        ConfirmEmployeeOverlay.IsVisible = false;
        AddEmployeeOverlay.IsVisible = true;
    }

    private void OnConfirmAddEmployeeClicked(object sender, EventArgs e)
    {
        if (_pendingEmployee == null)
            return;

        _employees.Add(_pendingEmployee);

        ClearForm();

        ConfirmEmployeeOverlay.IsVisible = false;
        RenderEmployees(SearchEmployeeBar.Text ?? "");
    }

    private List<string> GetSelectedDaysWithTimes()
    {
        var days = new List<string>();

        if (MondayCheckBox.IsChecked)
            days.Add($"Monday: {FormatTime(MondayStartTimePicker.Time)} - {FormatTime(MondayEndTimePicker.Time)}");

        if (TuesdayCheckBox.IsChecked)
            days.Add($"Tuesday: {FormatTime(TuesdayStartTimePicker.Time)} - {FormatTime(TuesdayEndTimePicker.Time)}");

        if (WednesdayCheckBox.IsChecked)
            days.Add($"Wednesday: {FormatTime(WednesdayStartTimePicker.Time)} - {FormatTime(WednesdayEndTimePicker.Time)}");

        if (ThursdayCheckBox.IsChecked)
            days.Add($"Thursday: {FormatTime(ThursdayStartTimePicker.Time)} - {FormatTime(ThursdayEndTimePicker.Time)}");

        if (FridayCheckBox.IsChecked)
            days.Add($"Friday: {FormatTime(FridayStartTimePicker.Time)} - {FormatTime(FridayEndTimePicker.Time)}");

        if (SaturdayCheckBox.IsChecked)
            days.Add($"Saturday: {FormatTime(SaturdayStartTimePicker.Time)} - {FormatTime(SaturdayEndTimePicker.Time)}");

        if (SundayCheckBox.IsChecked)
            days.Add($"Sunday: {FormatTime(SundayStartTimePicker.Time)} - {FormatTime(SundayEndTimePicker.Time)}");

        if (days.Count == 0)
            days.Add($"Monday: {FormatTime(MondayStartTimePicker.Time)} - {FormatTime(MondayEndTimePicker.Time)}");

        return days;
    }

    private string FormatTime(TimeSpan time)
    {
        return DateTime.Today.Add(time).ToString("hh:mm tt");
    }

    private void ClearForm()
    {
        NameEntry.Text = "";
        EmailEntry.Text = "";
        SkillEntry.Text = "";
        MaxHoursEntry.Text = "40";

        _pendingSkills.Clear();
        SelectedSkillsLayout.Children.Clear();
        ConfirmSkillsLayout.Children.Clear();

        MondayCheckBox.IsChecked = true;
        TuesdayCheckBox.IsChecked = false;
        WednesdayCheckBox.IsChecked = false;
        ThursdayCheckBox.IsChecked = false;
        FridayCheckBox.IsChecked = false;
        SaturdayCheckBox.IsChecked = false;
        SundayCheckBox.IsChecked = false;

        MondayStartTimePicker.Time = new TimeSpan(9, 0, 0);
        MondayEndTimePicker.Time = new TimeSpan(17, 0, 0);
        TuesdayStartTimePicker.Time = new TimeSpan(9, 0, 0);
        TuesdayEndTimePicker.Time = new TimeSpan(17, 0, 0);
        WednesdayStartTimePicker.Time = new TimeSpan(9, 0, 0);
        WednesdayEndTimePicker.Time = new TimeSpan(17, 0, 0);
        ThursdayStartTimePicker.Time = new TimeSpan(9, 0, 0);
        ThursdayEndTimePicker.Time = new TimeSpan(17, 0, 0);
        FridayStartTimePicker.Time = new TimeSpan(9, 0, 0);
        FridayEndTimePicker.Time = new TimeSpan(17, 0, 0);
        SaturdayStartTimePicker.Time = new TimeSpan(9, 0, 0);
        SaturdayEndTimePicker.Time = new TimeSpan(17, 0, 0);
        SundayStartTimePicker.Time = new TimeSpan(9, 0, 0);
        SundayEndTimePicker.Time = new TimeSpan(17, 0, 0);

        UpdateDayTimeVisibility();

        _pendingEmployee = null;
    }

    private async Task ShowAlert(string title, string message)
    {
        Page? page = Window?.Page;

        if (page != null)
        {
            await page.DisplayAlert(title, message, "OK");
        }
    }

    private class EmployeeModel
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public List<string> Skills { get; set; } = new();
        public int MaxHours { get; set; }
        public List<string> Availability { get; set; } = new();
    }
}