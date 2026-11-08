using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace SteelFlashcards;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class LearnSelectionPage : Page
{
    public LearnSelectionViewModel ViewModel;
    
    public LearnSelectionPage()
    {
        ViewModel = (App.ServiceProvider?.GetService<LearnSelectionViewModel>()) ?? throw new ApplicationException("Dev: Missing LearnSelectionViewModel Service");
        InitializeComponent();
    }

    private void ItemContainer_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (sender is not FrameworkElement element)
        {
            return;
        }

        element.MinWidth = Math.Max(element.MinWidth, element.ActualWidth);
    }

    private void ItemsView_SelectionChanged(ItemsView sender, ItemsViewSelectionChangedEventArgs args)
    {
        ViewModel.SelectedTags.Clear();
        foreach (var item in sender.SelectedItems)
            if (item is DictionaryTag tag)
                ViewModel.SelectedTags.Add(tag);
    }

    private void SelectAll_Click(object sender, RoutedEventArgs e)
    {
        TagsView.SelectAll();
    }

    private void DeselectAll_Click(object sender, RoutedEventArgs e)
    {
        TagsView.DeselectAll();
    }
}
