using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;

namespace SteelFlashcards;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class LearnPage : Page
{
    public LearnViewModel ViewModel;
    
    public LearnPage()
    {
        var viewModel = App.ServiceProvider?.GetService<LearnViewModel>();
        if (viewModel == null)
            throw new ApplicationException("Dev: Missing MainViewModel Service");
        ViewModel = viewModel;
        this.InitializeComponent();
    }

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

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.SaveAnswers();
    }
}
