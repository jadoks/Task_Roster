using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Task_Roster.Services;
using SkiaSharp.Views.Maui.Controls.Hosting;
using Microcharts.Maui;
namespace Task_Roster;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMicrocharts()
            .UseSkiaSharp()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif
        
        builder.Services.AddSingleton<DatabaseService>();

        return builder.Build();
    }
}