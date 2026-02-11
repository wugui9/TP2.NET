using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Gauniv.WpfClient.Services;
using Gauniv.WpfClient.ViewModels;

namespace Gauniv.WpfClient;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // 配置 HttpClient
        services.AddHttpClient<IAuthService, AuthService>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:5000"); 
        });

        services.AddHttpClient<IGameService, GameService>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:5000"); 
        });

        services.AddSingleton<INavigationService, NavigationService>();

        services.AddTransient<MainViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<GameListViewModel>();
        services.AddTransient<GameDetailsViewModel>();

        services.AddSingleton<MainWindow>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}

