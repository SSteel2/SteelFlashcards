using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LanguageLearn2
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private MainWindow m_window;
        private NavigationService m_navigationService;

        public static IServiceProvider ServiceProvider { get; private set; }

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
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            ServiceProvider = RegisterServices();
            m_window = new MainWindow(m_navigationService);
            m_window.Activate();
        }

        private IServiceProvider RegisterServices()
        {
            m_navigationService = new NavigationService();
            m_navigationService.Configure(nameof(LearnPage), typeof(LearnPage));
            m_navigationService.Configure(nameof(DictionariesPage), typeof(DictionariesPage));
            m_navigationService.Configure(nameof(EditPage), typeof(EditPage));

            var services = new ServiceCollection();
            services.AddSingleton<INavigationService>(m_navigationService);
            services.AddSingleton<IDataService, DataService>();
            services.AddTransient<LearnViewModel>();
            services.AddTransient<DictionariesViewModel>();
            services.AddTransient<EditViewModel>();
            return services.BuildServiceProvider();
        }
    }
}
