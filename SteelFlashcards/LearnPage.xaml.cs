using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LanguageLearn2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LearnPage : Page
    {
        public LearnPage()
        {
            ViewModel = App.ServiceProvider.GetService<LearnViewModel>();
            if (ViewModel == null)
                throw new ApplicationException("Dev: Missing MainViewModel Service");
            this.InitializeComponent();
        }

        public LearnViewModel ViewModel;

        private void GuessBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                ViewModel.AcceptAnswer(((TextBox)sender).Text);
                ((TextBox)sender).Text = string.Empty;
                AnswersListView.ScrollIntoView(AnswersListView.Items[^1]);
            }
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            if (AnswersListView.Items.Count > 0)
                AnswersListView.ScrollIntoView(AnswersListView.Items[^1]);
        }
    }
}
