using Microsoft.Maui.Controls.Shapes;

namespace Task_Roster.Views;

public partial class ReportsPage : ContentPage
{
    public List<string> Periods { get; set; } = new() { "This Week" };

    public ReportsPage()
    {
        InitializeComponent();
        BindingContext = this;

        LoadStats();
        LoadCharts();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private void LoadStats()
    {
        StatsGrid.Children.Add(CreateStatCard("Total Staff", "3", "👥", "#DBEAFE", "#2563EB", 0, 0));
        StatsGrid.Children.Add(CreateStatCard("Total Shifts", "5", "📅", "#DCFCE7", "#16A34A", 1, 0));
        StatsGrid.Children.Add(CreateStatCard("Task Rate", "0%", "✓", "#F3E8FF", "#A855F7", 0, 1));
        StatsGrid.Children.Add(CreateStatCard("Accept Rate", "80%", "↗", "#FEF3C7", "#F59E0B", 1, 1));
    }

    private View CreateStatCard(string title, string value, string icon, string bg, string iconColor, int col, int row)
    {
        var iconCircle = new Border
        {
            WidthRequest = 36,
            HeightRequest = 36,
            BackgroundColor = Color.FromArgb(bg),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 18 },
            Content = new Label
            {
                Text = icon,
                FontSize = 18,
                TextColor = Color.FromArgb(iconColor),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            }
        };

        var textStack = new VerticalStackLayout
        {
            Spacing = 2,
            Children =
            {
                new Label
                {
                    Text = title,
                    FontSize = 12,
                    TextColor = Color.FromArgb("#6B7280")
                },
                new Label
                {
                    Text = value,
                    FontSize = 22,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.Black
                }
            }
        };

        Grid.SetColumn(textStack, 1);

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition()
            },
            ColumnSpacing = 12,
            Children =
            {
                iconCircle,
                textStack
            }
        };

        var card = new Border
        {
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#E5E7EB"),
            Padding = 16,
            HeightRequest = 84,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Content = grid
        };

        Grid.SetColumn(card, col);
        Grid.SetRow(card, row);

        return card;
    }

    private void LoadCharts()
    {
        ChartsContainer.Children.Add(CreateChartCard("Hours by Employee", new VerticalBarChartDrawable(
            new[] { 16d, 8d, 8d },
            new[] { "John", "Sarah", "David" },
            "#3B82F6",
            16)));

        ChartsContainer.Children.Add(CreateChartCard("Shifts by Location", new HorizontalBarChartDrawable(
            new[] { 3d, 2d },
            new[] { "Downtown\nStore", "Westside Cafe" },
            "#10B981",
            3)));

        ChartsContainer.Children.Add(CreateChartCard("Shift Status Distribution", new PieChartDrawable()));

        ChartsContainer.Children.Add(CreateChartCard("Tasks by Priority", new VerticalBarChartDrawable(
            new[] { 3d, 2d, 0d },
            new[] { "High", "Medium", "Low" },
            "#F59E0B",
            3,
            true)));
    }

    private View CreateChartCard(string title, IDrawable drawable)
    {
        return new Border
        {
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#E5E7EB"),
            Padding = 16,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Content = new VerticalStackLayout
            {
                Spacing = 14,
                Children =
                {
                    new Label
                    {
                        Text = title,
                        FontSize = 14,
                        TextColor = Color.FromArgb("#1F2937")
                    },
                    new GraphicsView
                    {
                        Drawable = drawable,
                        HeightRequest = drawable is PieChartDrawable ? 230 : 220
                    }
                }
            }
        };
    }
}

public class VerticalBarChartDrawable : IDrawable
{
    private readonly double[] _values;
    private readonly string[] _labels;
    private readonly string _barColor;
    private readonly double _max;
    private readonly bool _showLegend;

    public VerticalBarChartDrawable(double[] values, string[] labels, string barColor, double max, bool showLegend = false)
    {
        _values = values;
        _labels = labels;
        _barColor = barColor;
        _max = max;
        _showLegend = showLegend;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float left = 58;
        float top = 12;
        float right = dirtyRect.Width - 10;
        float bottom = dirtyRect.Height - (_showLegend ? 42 : 24);
        float width = right - left;
        float height = bottom - top;

        canvas.StrokeColor = Color.FromArgb("#D1D5DB");
        canvas.StrokeSize = 1;
        canvas.FontColor = Color.FromArgb("#6B7280");
        canvas.FontSize = 12;

        int gridLines = 4;

        for (int i = 0; i <= gridLines; i++)
        {
            float y = bottom - (height / gridLines * i);
            double value = _max / gridLines * i;

            canvas.DrawLine(left, y, right, y);

            string label = _max == 3
                ? value.ToString("0.##")
                : value.ToString("0");

            canvas.DrawString(label, 0, y - 8, left - 8, 18, HorizontalAlignment.Right, VerticalAlignment.Center);
        }

        canvas.StrokeColor = Color.FromArgb("#9CA3AF");
        canvas.DrawLine(left, top, left, bottom);
        canvas.DrawLine(left, bottom, right, bottom);

        float slot = width / _values.Length;
        float barWidth = 34;

        canvas.FillColor = Color.FromArgb(_barColor);

        for (int i = 0; i < _values.Length; i++)
        {
            float barHeight = (float)(_values[i] / _max * height);
            float x = left + slot * i + (slot - barWidth) / 2;
            float y = bottom - barHeight;

            canvas.FillRectangle(x, y, barWidth, barHeight);
            canvas.FontColor = Color.FromArgb("#6B7280");
            canvas.DrawString(_labels[i], left + slot * i, bottom + 4, slot, 18, HorizontalAlignment.Center, VerticalAlignment.Top);
        }

        if (_showLegend)
        {
            float legendY = dirtyRect.Height - 24;

            canvas.FillColor = Color.FromArgb("#10B981");
            canvas.FillRectangle(left - 4, legendY, 12, 12);
            canvas.StrokeColor = Colors.Black;
            canvas.DrawRectangle(left - 4, legendY, 12, 12);
            canvas.FontColor = Color.FromArgb("#10B981");
            canvas.DrawString("Completed", left + 14, legendY - 3, 90, 18, HorizontalAlignment.Left, VerticalAlignment.Center);

            canvas.FillColor = Color.FromArgb("#F59E0B");
            canvas.FillRectangle(left + 100, legendY, 12, 12);
            canvas.StrokeColor = Colors.Black;
            canvas.DrawRectangle(left + 100, legendY, 12, 12);
            canvas.FontColor = Color.FromArgb("#F59E0B");
            canvas.DrawString("Pending", left + 118, legendY - 3, 90, 18, HorizontalAlignment.Left, VerticalAlignment.Center);
        }
    }
}

public class HorizontalBarChartDrawable : IDrawable
{
    private readonly double[] _values;
    private readonly string[] _labels;
    private readonly string _barColor;
    private readonly double _max;

    public HorizontalBarChartDrawable(double[] values, string[] labels, string barColor, double max)
    {
        _values = values;
        _labels = labels;
        _barColor = barColor;
        _max = max;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float left = 100;
        float top = 20;
        float right = dirtyRect.Width - 10;
        float bottom = dirtyRect.Height - 30;
        float width = right - left;
        float height = bottom - top;

        canvas.StrokeColor = Color.FromArgb("#D1D5DB");
        canvas.StrokeSize = 1;
        canvas.FontColor = Color.FromArgb("#6B7280");
        canvas.FontSize = 12;

        int gridLines = 4;

        for (int i = 0; i <= gridLines; i++)
        {
            float x = left + width / gridLines * i;
            double value = _max / gridLines * i;

            canvas.DrawLine(x, top, x, bottom);
            canvas.DrawString(value.ToString("0.##"), x - 16, bottom + 4, 32, 18, HorizontalAlignment.Center, VerticalAlignment.Top);
        }

        canvas.StrokeColor = Color.FromArgb("#9CA3AF");
        canvas.DrawLine(left, top, left, bottom);
        canvas.DrawLine(left, bottom, right, bottom);

        float row = height / _values.Length;
        float barHeight = 64;

        canvas.FillColor = Color.FromArgb(_barColor);

        for (int i = 0; i < _values.Length; i++)
        {
            float y = top + row * i + (row - barHeight) / 2;
            float barWidth = (float)(_values[i] / _max * width);

            canvas.FontColor = Color.FromArgb("#6B7280");
            canvas.DrawString(_labels[i], 0, y + 10, left - 8, 36, HorizontalAlignment.Right, VerticalAlignment.Center);

            canvas.FillColor = Color.FromArgb(_barColor);
            canvas.FillRectangle(left, y, barWidth, barHeight);
        }
    }
}

public class PieChartDrawable : IDrawable
{
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float centerX = dirtyRect.Width / 2;
        float centerY = dirtyRect.Height / 2 + 12;
        float radius = 78;

        DrawSlice(canvas, centerX, centerY, radius, 90, 288, "#10B981");
        DrawSlice(canvas, centerX, centerY, radius, 18, 72, "#F59E0B");

        canvas.StrokeColor = Colors.White;
        canvas.StrokeSize = 1;
        canvas.DrawLine(centerX, centerY, centerX + radius, centerY);

        canvas.FontSize = 13;

        canvas.FontColor = Color.FromArgb("#10B981");
        canvas.DrawString("Accepted: 4", 0, centerY - 70, 100, 20, HorizontalAlignment.Left, VerticalAlignment.Center);

        canvas.FontColor = Color.FromArgb("#EF4444");
        canvas.DrawString("Declined:", dirtyRect.Width - 82, centerY - 22, 80, 20, HorizontalAlignment.Left, VerticalAlignment.Center);

        canvas.FontColor = Color.FromArgb("#F59E0B");
        canvas.DrawString("Pending: 1", dirtyRect.Width - 100, centerY + 36, 100, 20, HorizontalAlignment.Left, VerticalAlignment.Center);
    }

    private void DrawSlice(ICanvas canvas, float cx, float cy, float radius, float startAngle, float sweepAngle, string color)
    {
        var path = new PathF();
        path.MoveTo(cx, cy);

        int steps = 60;

        for (int i = 0; i <= steps; i++)
        {
            float angle = startAngle + (sweepAngle * i / steps);
            float radians = angle * MathF.PI / 180f;

            float x = cx + radius * MathF.Cos(radians);
            float y = cy + radius * MathF.Sin(radians);

            path.LineTo(x, y);
        }

        path.Close();

        canvas.FillColor = Color.FromArgb(color);
        canvas.FillPath(path);
    }
}