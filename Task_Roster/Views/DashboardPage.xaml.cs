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
        await SwitchTabAsync(index, syncNav: false);
    }

    private async Task SwitchTabAsync(int index, bool syncNav = true)
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

        if (syncNav)
            await BottomNav.SetActiveTab(index);
    }

    public async Task OpenScheduleQuickActionAsync()
    {
        await MainContent.FadeTo(0, 100);

        var scheduleView = new ScheduleView();
        MainContent.Content = scheduleView;

        await MainContent.FadeTo(1, 120);

        await BottomNav.SetActiveTab(1);

        scheduleView.OpenAddShiftFromQuickAction();
    }

    public async Task OpenTeamQuickActionAsync()
    {
        await MainContent.FadeTo(0, 100);

        var teamView = new TeamView();
        MainContent.Content = teamView;

        await MainContent.FadeTo(1, 120);

        await BottomNav.SetActiveTab(2);

        teamView.OpenAddEmployeeFromQuickAction();
    }
}