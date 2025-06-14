using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LanguageLearn2
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private static MainWindow? m_window;
        private NavigationService? m_navigationService;

        public static MainWindow? MainWindow { get { return m_window; } }

        public static IServiceProvider? ServiceProvider { get; private set; }

        /// <summary>
        /// Initializes the singleton application object. This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
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

        private IServiceProvider RegisterServices()
        {
            m_navigationService = new NavigationService();
            m_navigationService.Configure(nameof(LearnPage), typeof(LearnPage));
            m_navigationService.Configure(nameof(LearnSelectionPage), typeof(LearnSelectionPage));
            m_navigationService.Configure(nameof(DictionariesPage), typeof(DictionariesPage));
            m_navigationService.Configure(nameof(EditPage), typeof(EditPage));
            m_navigationService.Configure(nameof(StatisticsPage), typeof(StatisticsPage));

            var services = new ServiceCollection();
            services.AddSingleton<INavigationService>(m_navigationService);
            services.AddSingleton<IDataService, DataService>();
            services.AddTransient<LearnViewModel>();
            services.AddTransient<LearnSelectionViewModel>();
            services.AddTransient<DictionariesViewModel>();
            services.AddTransient<EditViewModel>();
            services.AddTransient<StatisticsViewModel>();
            return services.BuildServiceProvider();
        }
    }
}
