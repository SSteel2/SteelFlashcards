using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace SteelFlashcards;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class StatisticsTagPage : Page
{
    public StatisticsTagViewModel ViewModel;

    public StatisticsTagPage()
    {
        var viewModel = App.ServiceProvider?.GetService<StatisticsTagViewModel>();
        if (viewModel == null)
            throw new ApplicationException("Dev: Missing StatisticsTagViewModel Service");
        ViewModel = viewModel;
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        ViewModel.InitializeTagStatistic(e.Parameter as string);
        base.OnNavigatedTo(e);
    }
}
