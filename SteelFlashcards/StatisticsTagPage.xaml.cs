using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LanguageLearn2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StatisticsTagPage : Page
    {
        public StatisticsTagViewModel ViewModel;

        public StatisticsTagPage()
        {
            ViewModel = App.ServiceProvider.GetService<StatisticsTagViewModel>();
            if (ViewModel == null)
                throw new ApplicationException("Dev: Missing StatisticsViewModel Service");
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.InitializeTagStatistic(e.Parameter as string);
            base.OnNavigatedTo(e);
        }
    }
}
