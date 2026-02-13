using System.Net;
using System.Net.Http;
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
        const string baseUrl = "http://localhost:5231";

        var cookieContainer = new CookieContainer();
        var handler = new HttpClientHandler
        {
            CookieContainer = cookieContainer,
            UseCookies = true
        };

        // Shared HttpClient with cookie support
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(baseUrl)
        };

        // Auth and Game services as singletons (share login state)
        services.AddSingleton<IAuthService>(new AuthService(httpClient));
        services.AddSingleton<IGameService>(new GameService(httpClient));

        services.AddSingleton<INavigationService, NavigationService>();

        services.AddTransient<MainViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<GameListViewModel>();
        services.AddTransient<GameDetailsViewModel>();
        services.AddTransient<ProfileViewModel>();

        services.AddSingleton<MainWindow>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}

