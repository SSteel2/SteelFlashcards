using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace SteelFlashcards;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private static MainWindow? m_window;
    private NavigationService? m_navigationService;

    public static MainWindow? MainWindow { get { return m_window; } }

    internal static IServiceProvider? ServiceProvider { get; private set; }

    /// <summary>
    /// Initializes the singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        ServiceProvider = RegisterServices();
        if (m_navigationService == null)
            throw new ApplicationException("Navigation service failed to initiazlize");
        m_window = new MainWindow(m_navigationService);
        m_window.Activate();
    }

    private ServiceProvider RegisterServices()
    {
        m_navigationService = new NavigationService();
        m_navigationService.Configure(nameof(LearnPage), typeof(LearnPage));
        m_navigationService.Configure(nameof(LearnSelectionPage), typeof(LearnSelectionPage));
        m_navigationService.Configure(nameof(DictionariesPage), typeof(DictionariesPage));
        m_navigationService.Configure(nameof(EditPage), typeof(EditPage));
        m_navigationService.Configure(nameof(StatisticsPage), typeof(StatisticsPage));
        m_navigationService.Configure(nameof(StatisticsTagPage), typeof(StatisticsTagPage));

        var services = new ServiceCollection();
        services.AddSingleton<INavigationService>(m_navigationService);
        services.AddSingleton<IDataService, DataService>();
        services.AddTransient<LearnViewModel>();
        services.AddTransient<LearnSelectionViewModel>();
        services.AddTransient<DictionariesViewModel>();
        services.AddTransient<EditViewModel>();
        services.AddTransient<StatisticsViewModel>();
        services.AddTransient<StatisticsTagViewModel>();
        return services.BuildServiceProvider();
    }
}
