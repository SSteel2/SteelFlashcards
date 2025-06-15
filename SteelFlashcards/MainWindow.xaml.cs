using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LanguageLearn2
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private INavigationService _navigationService;

        public MainWindow(INavigationService navigationService)
        {
            _navigationService = navigationService;
            InitializeComponent();
            _navigationService.SetMainFrame(RootFrame);
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var navigationViewItem = args.SelectedItem as NavigationViewItem;
            if (navigationViewItem != null)
            {
                _navigationService.NavigateTo(navigationViewItem.Tag as string);
                NavigationViewBar.Header = navigationViewItem.Content;
            }
        }

        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new ApplicationException($"Failed loading: {e.SourcePageType.FullName}");
        }
    }
}
