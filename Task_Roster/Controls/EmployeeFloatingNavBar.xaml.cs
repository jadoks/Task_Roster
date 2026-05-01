namespace Task_Roster.Controls;

public partial class EmployeeFloatingNavBar : ContentView
{
    public event EventHandler<int>? TabChanged;

    private int _selectedIndex;

    private const double IndicatorY = 8;

    private readonly string[] _icons =
    {
        "home.svg",
        "settings.svg"
    };

    private ImageButton[] Buttons => new[]
    {
        HomeButton,
        SettingsButton
    };

    private Label[] Labels => new[]
    {
        HomeLabel,
        SettingsLabel
    };

    public EmployeeFloatingNavBar()
    {
        InitializeComponent();

        Loaded += async (_, _) =>
        {
            await Task.Delay(100);
            IndicatorIcon.Source = _icons[_selectedIndex];
            UpdateButtonVisibility();
            await MoveIndicator(_selectedIndex, false);
        };

        SizeChanged += async (_, _) =>
        {
            await MoveIndicator(_selectedIndex, false);
        };
    }

    private async void HomeClicked(object sender, EventArgs e)
    {
        await SelectTab(0);
    }

    private async void SettingsClicked(object sender, EventArgs e)
    {
        await SelectTab(1);
    }

    private async Task SelectTab(int index)
    {
        if (_selectedIndex == index)
            return;

        _selectedIndex = index;
        IndicatorIcon.Source = _icons[index];

        UpdateButtonVisibility();

        await Indicator.ScaleTo(1.10, 80, Easing.CubicOut);
        await MoveIndicator(index, true);
        await Indicator.ScaleTo(1.0, 80, Easing.CubicIn);

        TabChanged?.Invoke(this, index);
    }

    private void UpdateButtonVisibility()
    {
        var buttons = Buttons;
        var labels = Labels;

        for (int i = 0; i < buttons.Length; i++)
        {
            if (i == _selectedIndex)
            {
                buttons[i].Opacity = 0;
                labels[i].Opacity = 0;
                buttons[i].InputTransparent = true;
            }
            else
            {
                buttons[i].Opacity = 0.45;
                labels[i].Opacity = 1;
                labels[i].TextColor = Color.FromArgb("#8A8A8A");
                buttons[i].InputTransparent = false;
            }
        }
    }

    private async Task MoveIndicator(int index, bool animated)
    {
        if (Width <= 0)
            return;

        double outerPadding = 48;
        double availableWidth = Width - outerPadding;
        double tabWidth = availableWidth / 2;

        double centerX = 8 + (tabWidth * index) + (tabWidth / 2);
        double indicatorX = centerX - 35;

        if (!animated)
        {
            Indicator.TranslationX = indicatorX;
            Indicator.TranslationY = IndicatorY;
            return;
        }

        await Indicator.TranslateTo(indicatorX, IndicatorY, 240, Easing.CubicOut);
    }
}