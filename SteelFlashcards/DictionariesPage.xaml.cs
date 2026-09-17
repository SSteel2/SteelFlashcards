using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SteelFlashcards;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class DictionariesPage : Page
{
    public DictionariesViewModel ViewModel;

    public DictionariesPage()
    {
        ViewModel = App.ServiceProvider.GetService<DictionariesViewModel>();
        if (ViewModel == null)
            throw new ApplicationException("Dev: Missing DictionariesViewModel Service");
        InitializeComponent();

        ViewModel.NewDictionaryCompleted += (_, _) => NewDictionaryFlyout.Hide();
        ViewModel.RenameDictionaryCompleted += (_, _) => RenameDictionaryFlyout.Hide();
    }

    private void NewDictionaryBox_KeyUp(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter && ViewModel.NewDictionaryCommand.CanExecute(null))
            ViewModel.NewDictionaryCommand.Execute(null);
    }

    private void RenameDictionaryBox_KeyUp(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter && ViewModel.RenameDictionaryCommand.CanExecute(null))
            ViewModel.RenameDictionaryCommand.Execute(null);
    }
}
