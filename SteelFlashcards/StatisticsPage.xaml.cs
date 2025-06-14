using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LanguageLearn2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StatisticsPage : Page
    {
        public StatisticsViewModel ViewModel;
        
        public StatisticsPage()
        {
            ViewModel = App.ServiceProvider.GetService<StatisticsViewModel>();
            if (ViewModel == null)
                throw new ApplicationException("Dev: Missing StatisticsViewModel Service");
            InitializeComponent();
        }
    }
}
