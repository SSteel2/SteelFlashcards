using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;

namespace SteelFlashcards;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class DictionariesPage : Page
{
    public DictionariesViewModel ViewModel;

    public DictionariesPage()
    {
        ViewModel = (App.ServiceProvider?.GetService<DictionariesViewModel>()) ?? throw new ApplicationException("Dev: Missing DictionariesViewModel Service");
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
