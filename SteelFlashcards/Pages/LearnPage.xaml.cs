using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Specialized;

namespace SteelFlashcards;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class LearnPage : Page
{
    public LearnViewModel ViewModel;
    
    public LearnPage()
    {
        ViewModel = (App.ServiceProvider?.GetService<LearnViewModel>()) ?? throw new ApplicationException("Dev: Missing MainViewModel Service");
        InitializeComponent();
        ViewModel.Answers.CollectionChanged += Answers_CollectionChanged;
    }

    private void Answers_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action != NotifyCollectionChangedAction.Add || e.NewItems is not { Count: > 0 })
            return;

        DispatcherQueue.TryEnqueue(() => AnswersListView.ScrollIntoView(e.NewItems[^1]));
    }

    private void GuessBox_KeyUp(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
            ViewModel.AcceptAnswerCommand.Execute(null);
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.Answers.CollectionChanged -= Answers_CollectionChanged;
        ViewModel.SaveAnswers();
    }

    private void HistoryVisibilityButton_Click(object sender, RoutedEventArgs e)
    {
        if (AnswersListView.Visibility == Visibility.Visible)
        {
            AnswersListView.Visibility = Visibility.Collapsed;
            HistoryColumn.Width = new GridLength(0);
            HistoryVisibilityButton.Content = "Show History";
        }
        else
        {
            AnswersListView.Visibility = Visibility.Visible;
            HistoryColumn.Width = new GridLength(360);
            HistoryVisibilityButton.Content = "Hide History";
        }
    }
}
