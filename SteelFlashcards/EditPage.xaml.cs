using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Extensions.DependencyInjection;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LanguageLearn2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EditPage : Page
    {
        public EditViewModel ViewModel;

        public EditPage()
        {
            ViewModel = App.ServiceProvider.GetService<EditViewModel>();
            if (ViewModel == null)
                throw new ApplicationException("Dev: Missing EditViewModel Service");
            this.InitializeComponent();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.EditWordEntry(((WordEntry)((FrameworkElement)((FrameworkElement)e.OriginalSource).Parent).Tag).LocalId);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.DeleteWordEntry(((WordEntry)((FrameworkElement)((FrameworkElement)e.OriginalSource).Parent).Tag).LocalId);
        }

        private void NewTags_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter && ViewModel.CanExecuteAcceptWordEntry())
            {
                WordEntry changedWordEntry = ViewModel.AcceptWordEntryInternal();
                NewWord.Text = string.Empty;
                NewMeaning.Text = string.Empty;
                NewTags.Text = string.Empty;
                NewWord.Focus(FocusState.Programmatic);
                WordsListView.ScrollIntoView(changedWordEntry);
            }
        }
    }
}
