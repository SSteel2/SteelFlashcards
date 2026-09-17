using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.Extensions.DependencyInjection;

namespace SteelFlashcards;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class EditPage : Page
{
    public EditViewModel ViewModel;

    public EditPage()
    {
        ViewModel = (App.ServiceProvider?.GetService<EditViewModel>()) ?? throw new ApplicationException("Dev: Missing EditViewModel Service");
        InitializeComponent();
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

    private void NewTags_GotFocus(object sender, RoutedEventArgs e)
    {
        ViewModel.FillLastUsedTags();
    }
}
