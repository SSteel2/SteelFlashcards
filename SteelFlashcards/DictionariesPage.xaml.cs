using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LanguageLearn2;

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
    }

    private void NewDictionaryAcceptButton_Click(object sender, RoutedEventArgs e)
    {
        NewDictionaryFlyout.Hide();
    }

    private void RenameDictionaryAcceptButton_Click(object sender, RoutedEventArgs e)
    {
        RenameDictionaryFlyout.Hide();
    }
}
