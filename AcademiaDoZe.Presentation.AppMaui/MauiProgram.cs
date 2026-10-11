using AcademiaDoZe.Presentation.AppMaui.Configuration;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;
using AcademiaDoZe.Presentation.AppMaui.Views;
using Microsoft.Extensions.Logging;

namespace AcademiaDoZe.Presentation.AppMaui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
           .ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
        });

        ConfigurationHelper.ConfigureServices(builder.Services);

        // ViewModels
        builder.Services.AddTransient<DashboardListViewModel>();
        builder.Services.AddTransient<LogradouroListViewModel>();
        builder.Services.AddTransient<LogradouroViewModel>();

        // Views
        builder.Services.AddTransient<DashboardListPage>();
        builder.Services.AddTransient<LogradouroListPage>();
        builder.Services.AddTransient<LogradouroPage>();
        // Views
        builder.Services.AddTransient<ConfigPage>();
        // ViewModels
        builder.Services.AddTransient<ColaboradorListViewModel>();
        builder.Services.AddTransient<ColaboradorViewModel>();

        // Views
        builder.Services.AddTransient<ColaboradorListPage>();
        builder.Services.AddTransient<ColaboradorPage>();
        // ViewModels
        builder.Services.AddTransient<AlunoListViewModel>();
        builder.Services.AddTransient<AlunoViewModel>();

        // Views
        builder.Services.AddTransient<AlunoListPage>();
        builder.Services.AddTransient<AlunoPage>();
#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}