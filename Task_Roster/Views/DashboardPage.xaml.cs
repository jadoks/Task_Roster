using Task_Roster.Views.DashboardTabs;

namespace Task_Roster.Views;

public partial class DashboardPage : ContentPage
{
    public DashboardPage()
    {
        InitializeComponent();

        MainContent.Content = new HomeView();
    }

    private async void OnTabChanged(object sender, int index)
    {
        await MainContent.FadeTo(0, 100);

        MainContent.Content = index switch
        {
            0 => new HomeView(),
            1 => new ScheduleView(), 
            2 => new TeamView(),
            3 => new SettingsView(),
            _ => new HomeView()
        };

        await MainContent.FadeTo(1, 120);
    }
}